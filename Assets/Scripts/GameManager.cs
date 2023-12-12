using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;
using System;
using UnityEngine.SceneManagement;
using UnityEditor;
using Unity.Mathematics;

public enum GameState
{
    InitializeGame,
    InitializeLevel,
    WaitingGameplayInput,
    Moving,
    LevelComplete,
    WaitingLevelCompleteInput,
    WaitingBombExploding,
    GameOver,
    WaitingGameOverInput,
    Paused
}

public enum SwipeDirection
{
    None = 0,
    Up = 1,
    Right = 2,
    Down = 3,
    Left = 4
}

public class GameManager : MonoBehaviour
{
    private GameState gameState;

    private List<Node> nodes;

    private Block player;

    private ICollection<Node> occupiedNodes;
    private ICollection<Target> targets;
    private ICollection<Bomb> bombs;
    private ICollection<StopBlock> stopBlocks;
    private ICollection<DirectionBlock> directionBlocks;
    private ICollection<ExplodeBombBlock> explodeBombBlocks;
    private GameConfiguration gameConfiguration;
    private int remainingMoves;
    private Goal goal;
    private CurrentLevelIndex currentLevel;
    [SerializeField] private Node nodePrefab;
    [SerializeField] private TMPro.TMP_Text remainingMovesText;
    [SerializeField] private TMPro.TMP_Text currentLevelText;
    [SerializeField] private TMPro.TMP_Text currentDifficultyText;
    private SoundManager soundManager;
    [SerializeField] private UnityEngine.UI.Button restartLevelButton;
    [SerializeField] private UnityEngine.UI.Button pauseButton;
    [SerializeField] private UnityEngine.UI.Button resumeButton;
    [SerializeField] private UnityEngine.UI.Button mainMenuButton;
    [SerializeField] private CurrentLevelIndex currentLevelPrefab;
    [SerializeField] private Animator nextLevelAnimator;
    [SerializeField] private Animator gameOverAnimator;
    [SerializeField] private UnityEngine.UI.Button settingsButton;
    [SerializeField] private UnityEngine.UI.Button settingsBackButton;
    [SerializeField] private SoundManager soundManagerPrefab;
    [SerializeField] private GameOverPanel gameOverPanel;
    [SerializeField] private LevelCompletePanel levelCompletePanel;
    [SerializeField] private UnityEngine.UI.Button creditsButton;
    [SerializeField] private UnityEngine.UI.Button creditsBackButton;
    [SerializeField] private TextAsset configuration;
    [SerializeField] private Canvas canvas;
    [SerializeField] private LevelManager levelManager;
    private bool updateCompleteLevelsInFile = false;
    private bool levelCompletePanelShown = false;
    private bool gameOverPanelShown = false;
    private float localScale = 1.0f;

    private Vector2 startTouchPosition;
    private Vector2 endTouchPosition;
    private bool touchStarted = false;
    private const float minSwipeDistance = 200;

    void Awake()
    {
        soundManager = GameObject.FindObjectOfType<SoundManager>();
        if (soundManager == null)
        {
            soundManager = Instantiate(soundManagerPrefab);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        SetGameState(GameState.InitializeGame);
        restartLevelButton.onClick.AddListener(RestartLevelButtonClicked);
        pauseButton.onClick.AddListener(PauseButtonClicked);
        resumeButton.onClick.AddListener(ResumeButtonClicked);
        mainMenuButton.onClick.AddListener(LoadMainMenu);
        settingsButton.onClick.AddListener(SettingsButtonClicked);
        settingsBackButton.onClick.AddListener(SettingsBackButtonClicked);

        levelCompletePanel.MainMenuButtonClicked += OnMainMenuButtonClicked;
        levelCompletePanel.RestartLevelButtonClicked += OnLevelCompleteReplayButtonClicked;
        levelCompletePanel.NextLevelButtonClicked += OnNextLevelButtonClicked;
        levelCompletePanel.LevelCompletePanelShown += OnLevelCompletePanelShown;

        gameOverPanel.MainMenuButtonClicked += OnMainMenuButtonClicked;
        gameOverPanel.RestartLevelButtonClicked += OnGameOverReplayButtonClicked;
        gameOverPanel.GameOverPanelShown += OnGameOverPanelShown;

        creditsButton.onClick.AddListener(() =>
        {
            soundManager.PlayConfirmSound();
        });

        creditsBackButton.onClick.AddListener(() =>
        {
            soundManager.PlayDeclineSound();
        });
    }

    private void SetGameState(GameState newState)
    {
        gameState = newState;

        switch (gameState)
        {
            case GameState.InitializeGame:
                InitializeGame();
                break;
            case GameState.InitializeLevel:
                InitializeLevel();
                break;
            case GameState.LevelComplete:
                SpawnLevelCompleteCanvas();
                break;
            case GameState.WaitingGameplayInput:
            case GameState.Moving:
            case GameState.WaitingLevelCompleteInput:
            case GameState.WaitingGameOverInput:
                break;
            case GameState.GameOver:
                SpawnGameOverCanvas();
                break;
        }
    }

    private void IncrementAndSaveCompletedLevels()
    {
        if (!updateCompleteLevelsInFile)
        {
            return;
        }

        var difficulty = gameConfiguration
            .Difficulties
            .FirstOrDefault(x => x.DifficultyLevel == currentLevel.DifficultyLevel);

        if (difficulty == null)
        {
            return;
        }

        var completedLevels = 0;

        switch (difficulty.DifficultyLevel)
        {
            case 1:
                completedLevels = PlayerPrefs.GetInt("completed3x3Levels", 0);
                break;
            case 2:
                completedLevels = PlayerPrefs.GetInt("completed4x4Levels", 0);
                break;
            case 3:
                completedLevels = PlayerPrefs.GetInt("completed5x5Levels", 0);
                break;
            case 4:
                completedLevels = PlayerPrefs.GetInt("completed6x6Levels", 0);
                break;
        }

        if (currentLevel.CurrentLevel == completedLevels + 1)
        {
            ++completedLevels;
            switch (difficulty.DifficultyLevel)
            {
                case 1:
                    PlayerPrefs.SetInt("completed3x3Levels", completedLevels);
                    break;
                case 2:
                    PlayerPrefs.SetInt("completed4x4Levels", completedLevels);
                    break;
                case 3:
                    PlayerPrefs.SetInt("completed5x5Levels", completedLevels);
                    break;
                case 4:
                    PlayerPrefs.SetInt("completed6x6Levels", completedLevels);
                    break;
            }
        }

        updateCompleteLevelsInFile = false;
    }

    private void InitializeGame()
    {
        var json = configuration.text;
        gameConfiguration = JsonUtility.FromJson<GameConfiguration>(json);

        occupiedNodes = new List<Node>();
        nodes = new List<Node>();
        targets = new List<Target>();
        bombs = new List<Bomb>();
        stopBlocks = new List<StopBlock>();
        directionBlocks = new List<DirectionBlock>();
        explodeBombBlocks = new List<ExplodeBombBlock>();

        soundManager.PlayMusicAudioSource();
        currentLevel = GameObject.FindObjectOfType<CurrentLevelIndex>();
        if (currentLevel == null)
        {
            currentLevel = Instantiate(currentLevelPrefab);
        }

        SetGameState(GameState.InitializeLevel);
    }

    private void ClearLevelGameObjects()
    {
        if (occupiedNodes.Any())
        {
            foreach (var occupiedBlock in occupiedNodes)
            {
                Destroy(occupiedBlock.gameObject);
            }

            occupiedNodes.Clear();
        }

        if (bombs.Any())
        {
            foreach (var bomb in bombs)
            {
                Destroy(bomb.gameObject);
            }

            bombs.Clear();
        }

        if (nodes.Any())
        {
            foreach (var node in nodes)
            {
                Destroy(node.gameObject);
            }
            nodes.Clear();
        }

        if (targets.Any())
        {
            foreach (var target in targets)
            {
                Destroy(target.gameObject);
            }

            targets.Clear();
        }

        if (stopBlocks.Any())
        {
            foreach (var stopBlock in stopBlocks)
            {
                Destroy(stopBlock.gameObject);
            }

            stopBlocks.Clear();
        }

        if (directionBlocks.Any())
        {
            foreach (var directionBlock in directionBlocks)
            {
                Destroy(directionBlock.gameObject);
            }

            directionBlocks.Clear();
        }

        if (explodeBombBlocks.Any())
        {
            foreach (var explodeBombBlock in explodeBombBlocks)
            {
                Destroy(explodeBombBlock.gameObject);
            }

            explodeBombBlocks.Clear();
        }

        if (player != null)
        {
            Destroy(player.gameObject);
        }

        if (goal != null)
        {
            Destroy(goal.gameObject);
        }
    }

    private Tuple<int, int> GetDifficultyGridSize()
    {
        switch (currentLevel.DifficultyLevel)
        {
            case 2:
                return new Tuple<int, int>(4, 4);
            case 3:
                return new Tuple<int, int>(5, 5);
            case 4:
                return new Tuple<int, int>(6, 6);
            case 1:
            default:
                return new Tuple<int, int>(3, 3);
        }
    }

    private void InitializeLevel()
    {
        ClearLevelGameObjects();

        var difficultyGrid = GetDifficultyGridSize();

        var level = gameConfiguration.Difficulties
            .FirstOrDefault(x => x.DifficultyLevel == currentLevel.DifficultyLevel)
            ?.Levels
            .FirstOrDefault(l => l.Index == currentLevel.CurrentLevel);

        if (level != null)
        {
            switch (currentLevel.DifficultyLevel)
            {
                case 1:
                    localScale = 1.4f;
                    break;
                case 2:
                    localScale = 1.1f;
                    break;
                case 3:
                    localScale = 0.85f;
                    break;
                case 4:
                    localScale = 0.7f;
                    break;
                default:
                    localScale = 1.0f;
                    break;
            }

            for (int x = 0; x < difficultyGrid.Item1; ++x)
            {
                for (int y = 0; y < difficultyGrid.Item2; ++y)
                {
                    var node = Instantiate(nodePrefab, new Vector2(x, y), Quaternion.identity);
                    node.GameBoardPosition = new Vector2(x, y);
                    nodes.Add(node);
                }
            }

            remainingMoves = level.Moves;

            occupiedNodes = levelManager.AddOccupiedBlocks(level.OccupiedBlockLocations, nodes);
            stopBlocks = levelManager.SpawnStopBlocks(level.StopBlockLocations, nodes);
            directionBlocks = levelManager.SpawnDirectionBlocks(level.DirectionBlockConfigs, nodes);
            bombs = levelManager.SpawnBombs(level.BombLocations, nodes);
            player = levelManager.SpawnPlayer(level.PlayerPosition, nodes);
            targets = levelManager.SpawnTargets(level.TargetLocations, nodes);
            goal = levelManager.SpawnGoal(level.GoalPosition, nodes, !targets.Any());
            explodeBombBlocks = levelManager.SpawnExplodeBombBlocks(level.ExplodeBombLocations, nodes);

            foreach (var bomb in bombs)
            {
                bomb.ExplosionComplete += OnExplosionComplete;
            }

            UpdateRemainingMoves(remainingMoves);
            UpdateCurrentLevel(level.Index);
            UpdateCurrentDifficulty(currentLevel.DifficultyLevel);

            ApplyScaling();

            var center = new Vector2(((float)difficultyGrid.Item1 * localScale) / 2 - localScale / 2, ((float)difficultyGrid.Item2 * localScale) / 2 - localScale / 2);

            Camera.main.transform.position = new Vector3(center.x, center.y, -10);
            canvas.transform.position = new Vector3(center.x, center.y, canvas.transform.position.z);
            ResetTouchPositions();
            levelCompletePanelShown = false;
            gameOverPanelShown = false;
            SetGameState(GameState.WaitingGameplayInput);
        }
    }

    private void ApplyScaling()
    {
        ApplyScaling(nodes.Select(x => x.gameObject).ToList());
        ApplyScaling(bombs.Select(x => x.gameObject).ToList());
        ApplyScaling(targets.Select(x => x.gameObject).ToList());
        ApplyScaling(stopBlocks.Select(x => x.gameObject).ToList());
        ApplyScaling(directionBlocks.Select(x => x.gameObject).ToList());
        ApplyScaling(explodeBombBlocks.Select(x => x.gameObject).ToList());

        player.gameObject.transform.position = new Vector2(player.transform.position.x * localScale, player.transform.position.y * localScale);
        player.gameObject.transform.localScale = new Vector3(localScale, localScale, localScale);

        goal.gameObject.transform.position = new Vector2(goal.transform.position.x * localScale, goal.transform.position.y * localScale);
        goal.gameObject.transform.localScale = new Vector3(localScale, localScale, localScale);
    }

    private void ApplyScaling(List<GameObject> gameObjects)
    {
        gameObjects.ForEach(x =>
        {
            x.gameObject.transform.position = new Vector2(x.transform.position.x * localScale, x.transform.position.y * localScale);
            x.gameObject.transform.localScale = new Vector3(
                x.gameObject.transform.localScale.x * localScale,
                x.gameObject.transform.localScale.y * localScale,
                x.gameObject.transform.localScale.z * localScale);
        });
    }
    private void SpawnLevelCompleteCanvas()
    {
        levelCompletePanel.InitializeButtons(currentLevel);
        ToggleMenuButtons(false);
        nextLevelAnimator.Play("ShowPanel", 0);
    }

    private void SpawnGameOverCanvas()
    {
        gameOverAnimator.Play("ShowPanel", 0);
        soundManager.PlayGameOverSound();
        SetGameState(GameState.WaitingGameOverInput);
    }

    private void OnNextLevelButtonClicked(object emptyObject) =>
        SpawnNextLevel();

    private void OnLevelCompletePanelShown(AnimationEvent animationEvent)
    {
        if(gameState == GameState.WaitingLevelCompleteInput)
        {
            levelCompletePanelShown = true;
        }
    }

    private void OnGameOverPanelShown(AnimationEvent animationEvent)
    {
        if(gameState == GameState.WaitingGameOverInput)
        {
            gameOverPanelShown = true;
        }
    }

    private void SpawnNextLevel()
    {
        ToggleMenuButtons(true);
        soundManager.PlayConfirmSound();
        if (currentLevel.CurrentLevel < 25)
        {
            ++currentLevel.CurrentLevel;
        }
        else
        {
            currentLevel.CurrentLevel = 1;
            ++currentLevel.DifficultyLevel;
        }

        nextLevelAnimator.Play("HidePanel", 0);
        levelCompletePanelShown = false;
        SetGameState(GameState.InitializeLevel);
    }

    private void ToggleMenuButtons(bool active)
    {
        pauseButton.interactable = active;
        settingsButton.interactable = active;
        restartLevelButton.interactable = active;
    }

    private void PauseButtonClicked()
    {
        ResetTouchPositions();
        soundManager.PlayConfirmSound();
        ToggleMenuButtons(false);
        SetGameState(GameState.Paused);
    }

    private void SettingsButtonClicked()
    {
        ResetTouchPositions();
        soundManager.PlayConfirmSound();
        ToggleMenuButtons(false);
        SetGameState(GameState.Paused);
    }

    private void ResumeButtonClicked()
    {
        soundManager.PlayDeclineSound();
        ToggleMenuButtons(true);
        SetGameState(GameState.WaitingGameplayInput);
    }

    private void SettingsBackButtonClicked()
    {
        soundManager.PlayDeclineSound();
        ToggleMenuButtons(true);
        SetGameState(GameState.WaitingGameplayInput);
        soundManager.SaveAudioPreferences();
    }

    private void OnMainMenuButtonClicked(object emptyObject) =>
        LoadMainMenu();

    private void LoadMainMenu()
    {
        soundManager.PlayDeclineSound();
        SceneManager.LoadSceneAsync("MenuScene");
    }

    private void RestartLevel() =>
        SetGameState(GameState.InitializeLevel);

    private void RestartLevelButtonClicked()
    {
        soundManager.PlayConfirmSound();
        RestartLevel();
    }

    private void OnLevelCompleteReplayButtonClicked(object emptyObject) =>
        HidePanelForRestartLevel(nextLevelAnimator);

    private void OnGameOverReplayButtonClicked(object emptyObject) =>
        HidePanelForRestartLevel(gameOverAnimator);

    private void HidePanelForRestartLevel(Animator animator)
    {
        soundManager.PlayConfirmSound();
        ToggleMenuButtons(true);
        animator.Play("HidePanel", 0);
        RestartLevel();
    }

    private void UpdateRemainingMoves(int moves)
    {
        if (remainingMovesText == null)
        {
            return;
        }

        remainingMovesText.text = moves.ToString();
    }

    private void UpdateCurrentLevel(int currentLevel)
    {
        if (currentLevelText == null)
        {
            return;
        }

        currentLevelText.text = currentLevel.ToString();
    }

    private void UpdateCurrentDifficulty(int currentDifficulty)
    {
        var difficulty = "";
        switch (currentDifficulty)
        {
            case (2):
                difficulty = "4x4";
                break;
            case (3):
                difficulty = "5x5";
                break;
            case (4):
                difficulty = "6x6";
                break;
            case (1):
            default:
                difficulty = "3x3";
                break;
        }

        currentDifficultyText.text = difficulty;
    }

    private SwipeDirection GetSwipeDirection()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            startTouchPosition = Input.GetTouch(0).position;
            touchStarted = true;
        }

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended && touchStarted)
        {
            endTouchPosition = Input.GetTouch(0).position;
            touchStarted = false;
            var positionDiff = endTouchPosition - startTouchPosition;

            if (Math.Abs(positionDiff.x) > Math.Abs(positionDiff.y) && Math.Abs(positionDiff.x) > minSwipeDistance)
            {
                if (startTouchPosition.x > endTouchPosition.x)
                {
                    return SwipeDirection.Left;
                }

                if (startTouchPosition.x < endTouchPosition.x)
                {
                    return SwipeDirection.Right;
                }
            }
            else if(Math.Abs(positionDiff.y) > minSwipeDistance)
            {
                if (startTouchPosition.y < endTouchPosition.y)
                {
                    return SwipeDirection.Up;
                }

                if (startTouchPosition.y > endTouchPosition.y)
                {
                    return SwipeDirection.Down;
                }
            }
        }

        return SwipeDirection.None;
    }

    // Update is called once per frame
    private void Update()
    {
        switch (gameState)
        {
            case GameState.WaitingGameplayInput:
                var swipeDirection = GetSwipeDirection();
                if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A) || swipeDirection == SwipeDirection.Left)
                {
                    MoveBlock(Vector2.left);
                }
                else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D) || swipeDirection == SwipeDirection.Right)
                {
                    MoveBlock(Vector2.right);
                }
                else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) || swipeDirection == SwipeDirection.Up)
                {
                    MoveBlock(Vector2.up);
                }
                else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S) || swipeDirection == SwipeDirection.Down)
                {
                    MoveBlock(Vector2.down);
                }
                break;

            case GameState.LevelComplete:
                if (updateCompleteLevelsInFile)
                {
                    IncrementAndSaveCompletedLevels();
                }

                ResetTouchPositions();
                SetGameState(GameState.WaitingLevelCompleteInput);
                break;
            
            case GameState.WaitingLevelCompleteInput:
                if (levelCompletePanelShown && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
                {
                    SpawnNextLevel();
                }

                break;
            
            case GameState.WaitingGameOverInput:
                if (gameOverPanelShown && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
                {
                    HidePanelForRestartLevel(gameOverAnimator);
                }

                break;
        }
    }

    private void ResetTouchPositions()
    {
        touchStarted = false;
        startTouchPosition = new Vector2(0, 0);
        endTouchPosition = new Vector2(0, 0);
    }

    private Node GetNodeAtPosition(Vector2 gameBoardPosition)
    {
        var possibleNodes = nodes
            .Where(n => !n.IsOccupied);

        if (!goal.GoalActive)
        {
            possibleNodes = possibleNodes
                .Where(n => !n.IsGoalNode);
        }

        return possibleNodes
            .FirstOrDefault(n => n.GameBoardPosition == gameBoardPosition);
    }

    private void MoveBlock(Vector2 direction)
    {
        SetGameState(GameState.Moving);

        var orderedNodes = nodes.OrderBy(x => x.Position.x).ThenBy(x => x.Position.y).ToList();
        if (direction == Vector2.right || direction == Vector2.up)
        {
            orderedNodes.Reverse();
        }

        var originalPosition = player.Node;
        var next = player.Node;
        var targetsToDelete = new List<Target>();
        var playerMovePath = new List<Vector3>();
        var bombHit = false;
        var blockDistance = 0;
        var blockDistances = new List<int>();
        var targetSoundQueueCount = 0;
        do
        {
            next = GetNodeAtPosition(next.GameBoardPosition + direction);
            ++blockDistance;
            if (next != null)
            {
                if (next.HasTarget)
                {
                    var target = targets.FirstOrDefault(t => t.Node == next);
                    targetsToDelete.Add(target);
                    blockDistances.Add(blockDistance);
                    ++targetSoundQueueCount;
                    next.HasTarget = false;
                }

                playerMovePath.Add(next.transform.position);
                player.Node = next;

                if (next.HasBomb)
                {
                    bombHit = true;
                    break;
                }

                if (next.IsDirectionBlockNode)
                {
                    var directionBlock = directionBlocks.
                        FirstOrDefault(x => x.Node == next);

                    if (directionBlock != null)
                    {
                        switch (directionBlock.BlockDirection)
                        {
                            case BlockDirection.Up:
                                direction = Vector2.up;
                                break;
                            case BlockDirection.Right:
                                direction = Vector2.right;
                                break;
                            case BlockDirection.Down:
                                direction = Vector2.down;
                                break;
                            case BlockDirection.Left:
                                direction = Vector2.left;
                                break;
                        }
                    }
                }
            }
        }
        while (next != null && !next.IsStopBlockNode);

        if (targetSoundQueueCount > 0)
        {
            soundManager.PlayTargetPickupSound(blockDistances, targetSoundQueueCount);
        }

        if (originalPosition != player.Node || playerMovePath.Any())
        {
            soundManager.PlayMoveSound();
            var sequence = DOTween.Sequence();
            sequence.Insert(0, player.transform.DOPath(playerMovePath.ToArray(), playerMovePath.Count * 0.15f).SetEase(Ease.Linear).OnWaypointChange((int index) =>
            {
                if (index > 0)
                {
                    var previousNode = playerMovePath[index - 1];
                    var targetToDelete = targetsToDelete
                        .Where(x => x.Node.Position.x == previousNode.x)
                        .FirstOrDefault(x => x.Node.Position.y == previousNode.y);

                    if (targetToDelete != null)
                    {
                        targets.Remove(targetToDelete);
                        Destroy(targetToDelete.gameObject);
                    }
                }
            }));

            sequence.OnComplete(() =>
            {
                if (playerMovePath.Count == 1 && targetsToDelete.Count == 1)
                {
                    targets.Remove(targetsToDelete[0]);
                    Destroy(targetsToDelete[0].gameObject);
                }

                if (!goal.GoalActive && !targets.Any())
                {
                    goal.SetGoalState(true);
                }

                var explodeBombs = nodes
                    .Where(x => x.IsExplodeBombNode)
                    .Where(x => x.Position.x == player.Node.Position.x)
                    .Any(x => x.Position.y == player.Node.Position.y);

                UpdateRemainingMoves(--remainingMoves);
                if (player.Node.Position.x == goal.Node.Position.x &&
                    player.Node.Position.y == goal.Node.Position.y &&
                    goal.GoalActive)
                {
                    updateCompleteLevelsInFile = true;
                    soundManager.PlayGoalSound();
                    SetGameState(GameState.LevelComplete);
                }
                else if (bombHit)
                {
                    Destroy(player.gameObject);

                    var bomb = bombs
                        .FirstOrDefault(b => b.Node == next);

                    if (bomb != null)
                    {
                        soundManager.PlayExplosionSound();
                        bomb.PlayExplosion();
                    }

                }
                else if (remainingMoves == 0)
                {
                    InitializeGameOver();
                }
                else if (explodeBombs && bombs.Any())
                {
                    soundManager.PlayExplosionSound();
                    foreach (var bomb in bombs)
                    {
                        bomb.PlayExplosion();
                    }

                    foreach (var node in nodes)
                    {
                        node.HasBomb = false;
                    }

                    SetGameState(GameState.WaitingGameplayInput);
                }
                else
                {
                    SetGameState(GameState.WaitingGameplayInput);
                }
            });
        }
        else
        {
            SetGameState(GameState.WaitingGameplayInput);
        }
    }

    private void OnExplosionComplete(Bomb bomb)
    {
        if (bomb.Node.Position.x == player.Node.Position.x &&
            bomb.Node.Position.y == player.Node.Position.y &&
            bomb.Node.HasBomb)
        {
            InitializeGameOver();
        }
        else
        {
            SetGameState(GameState.WaitingGameplayInput);
        }

        bombs.Remove(bomb);
        Destroy(bomb.gameObject);
    }

    private void InitializeGameOver()
    {
        ToggleMenuButtons(false);
        SetGameState(GameState.GameOver);
    }
}
