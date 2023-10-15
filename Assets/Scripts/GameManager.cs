using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;
using System;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEditor.Experimental.GraphView;

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
    [SerializeField] private Block blockPrefab;
    [SerializeField] private Target targetPrefab;
    [SerializeField] private Goal goalPrefab;
    [SerializeField] private StopBlock stopBlockPrefab;
    [SerializeField] private ExplodeBombBlock explodeBombBlockPrefab;
    [SerializeField] private DirectionBlock directionBlockPrefab;
    [SerializeField] private TMPro.TMP_Text remainingMovesText;
    [SerializeField] private TMPro.TMP_Text currentLevelText;
    [SerializeField] private TMPro.TMP_Text currentDifficultyText;
    [SerializeField] private Bomb bombPrefab;
    private const float travelTime = 0.2f;
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
    private string configurationFilePath = "";
    private bool updateCompleteLevelsInFile = false;

    void Awake()
    {
        configurationFilePath = Path.GetFullPath($@"{Application.dataPath}\Scripts\Levels.json");
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

        gameOverPanel.MainMenuButtonClicked += OnMainMenuButtonClicked;
        gameOverPanel.RestartLevelButtonClicked += OnGameOverReplayButtonClicked;
        
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

        if (currentLevel.CurrentLevel == difficulty.CompletedLevels + 1)
        {
            ++difficulty.CompletedLevels;

            var jsonToWrite = JsonUtility.ToJson(gameConfiguration, true);
            File.WriteAllText(configurationFilePath, jsonToWrite);
        }

        updateCompleteLevelsInFile = false;
    }

    private void InitializeGame()
    {
        using (var reader = new StreamReader(configurationFilePath))
        {
            var json = reader.ReadToEnd();
            gameConfiguration = JsonUtility.FromJson<GameConfiguration>(json);
        }

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
            for (int x = 0; x < difficultyGrid.Item1; ++x)
            {
                for (int y = 0; y < difficultyGrid.Item2; ++y)
                {
                    var node = Instantiate(nodePrefab, new Vector2(x, y), Quaternion.identity);
                    nodes.Add(node);
                }
            }

            remainingMoves = level.Moves;
            AddOccupiedBlocks(level.OccupiedBlockLocations);
            SpawnStopBlocks(level.StopBlockLocations);
            SpawnDirectionBlocks(level.DirectionBlockConfigs);
            SpawnBombs(level.BombLocations);
            SpawnPlayer(level.PlayerPosition);
            SpawnTargets(level.TargetLocations);
            SpawnGoal(level.GoalPosition);
            SpawnExplodeBombBlocks(level.ExplodeBombLocations);
            UpdateRemainingMoves(remainingMoves);
            UpdateCurrentLevel(level.Index);
            UpdateCurrentDifficulty(currentLevel.DifficultyLevel);

            var center = new Vector2((float)difficultyGrid.Item1 / 2 - 0.5f, (float)difficultyGrid.Item2 / 2 - 0.5f);

            Camera.main.transform.position = new Vector3(center.x, center.y, -10);
            SetGameState(GameState.WaitingGameplayInput);
        }
    }

    private void SpawnStopBlocks(Vector2[] stopBlockPositions)
    {
        foreach (var stopBlockPosition in stopBlockPositions)
        {
            var node = nodes
                .Where(n => !n.IsGoalNode)
                .Where(n => !n.IsOccupied)
                .Where(n => n.Position.x == stopBlockPosition.x)
                .FirstOrDefault(n => n.Position.y == stopBlockPosition.y);

            if (node != null)
            {
                var stopBlock = Instantiate(stopBlockPrefab, node.Position, Quaternion.identity);
                stopBlock.Init(node);
                stopBlocks.Add(stopBlock);
            }
        }
    }

    private void SpawnDirectionBlocks(DirectionBlockConfig[] directionBlockConfigs)
    {
        if (directionBlockConfigs == null)
        {
            return;
        }

        foreach (var directionBlockConfig in directionBlockConfigs)
        {
            var node = nodes
                .Where(n => !n.IsGoalNode)
                .Where(n => !n.IsOccupied)
                .Where(n => !n.IsStopBlockNode)
                .Where(n => n.Position.x == directionBlockConfig.BlockPosition.x)
                .FirstOrDefault(n => n.Position.y == directionBlockConfig.BlockPosition.y);

            if (node != null)
            {
                var directionBlock = Instantiate(directionBlockPrefab, node.Position, Quaternion.identity);
                directionBlock.Init(node, (BlockDirection)directionBlockConfig.BlockDirection);
                directionBlocks.Add(directionBlock);
            }
        }
    }

    private void AddOccupiedBlocks(Vector2[] occupiedBlockPositions)
    {
        foreach (var occupiedBlockPosition in occupiedBlockPositions)
        {
            var node = nodes
                .Where(n => !n.IsGoalNode)
                .Where(n => n.Position.x == occupiedBlockPosition.x)
                .FirstOrDefault(n => n.Position.y == occupiedBlockPosition.y);
            if (node != null)
            {
                node.SetNodeOccupied();
                occupiedNodes.Add(node);
            }
        }
    }

    private void SpawnGoal(Vector2 goalPosition)
    {
        var node = Instantiate(nodePrefab, goalPosition, Quaternion.identity);
        node.SetGoalNode();
        nodes.Add(node);
        goal = Instantiate(goalPrefab, goalPosition, Quaternion.identity);
        goal.Init(node);
        goal.SetGoalState(!targets.Any());
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

    private void OnNextLevelButtonClicked(object emptyObject)
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
        soundManager.PlayConfirmSound();
        ToggleMenuButtons(false);
        SetGameState(GameState.Paused);
    }

    private void SettingsButtonClicked()
    {
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

    private void OnLevelCompleteReplayButtonClicked(object emptyObject)
    {
        soundManager.PlayConfirmSound();
        ToggleMenuButtons(true);
        nextLevelAnimator.Play("HidePanel", 0);
        RestartLevel();
    }

    private void OnGameOverReplayButtonClicked(object emptyObject)
    {
        soundManager.PlayConfirmSound();
        ToggleMenuButtons(true);
        gameOverAnimator.Play("HidePanel", 0);
        RestartLevel();
    }

    private void SpawnBombs(Vector2[] bombLocations)
    {
        foreach (var bombLocation in bombLocations)
        {
            var node = nodes
                .Where(n => !n.IsGoalNode)
                .Where(n => !n.IsOccupied)
                .Where(n => n.Position.x == bombLocation.x)
                .FirstOrDefault(n => n.Position.y == bombLocation.y);

            if (node != null)
            {
                var bomb = Instantiate(bombPrefab, node.Position, Quaternion.identity);
                bomb.Init(node);
                bomb.ExplosionComplete += OnExplosionComplete;
                bombs.Add(bomb);
            }
        }
    }

    private void SpawnExplodeBombBlocks(Vector2[] explodeBombBlockLocations)
    {
        foreach (var explodeBombBlockLocation in explodeBombBlockLocations)
        {
            var node = nodes
                .Where(n => !n.IsGoalNode)
                .Where(n => !n.IsOccupied)
                .Where(n => n.Position.x == explodeBombBlockLocation.x)
                .FirstOrDefault(n => n.Position.y == explodeBombBlockLocation.y);

            if (node != null)
            {
                var explodeBombBlock = Instantiate(explodeBombBlockPrefab, node.Position, Quaternion.identity);
                explodeBombBlock.Init(node);
                explodeBombBlocks.Add(explodeBombBlock);
            }
        }
    }

    private void SpawnTargets(Vector2[] targetLocations)
    {
        foreach (var targetLocation in targetLocations)
        {
            var node = nodes
                .Where(n => !n.IsGoalNode)
                .Where(n => !n.IsOccupied)
                .Where(n => n.Position.x == targetLocation.x)
                .FirstOrDefault(n => n.Position.y == targetLocation.y);

            if (node != null)
            {
                var target = Instantiate(targetPrefab, node.Position, Quaternion.identity);
                target.Init(node);
                targets.Add(target);
            }
        }
    }

    private void SpawnPlayer(Vector2 position)
    {
        var node = nodes
            .Where(n => !n.IsGoalNode)
            .Where(n => !n.IsOccupied)
            .Where(n => n.Position.x == position.x)
            .FirstOrDefault(n => n.Position.y == position.y);

        if (node != null)
        {
            player = Instantiate(blockPrefab, node.Position, Quaternion.identity);
            player.Init(node);
        }
    }

    private void UpdateRemainingMoves(int moves)
    {
        if (remainingMovesText == null)
        {
            return;
        }

        var baseText = remainingMovesText.text.Split(':');
        remainingMovesText.text = $"{baseText[0]}: {moves}";
    }

    private void UpdateCurrentLevel(int currentLevel)
    {
        if (currentLevelText == null)
        {
            return;
        }

        var baseText = currentLevelText.text.Split(':');
        currentLevelText.text = $"{baseText[0]}: {currentLevel}";
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

        var baseText = currentDifficultyText.text.Split(':');
        currentDifficultyText.text = $"{baseText[0]}: {difficulty}";
    }

    // Update is called once per frame
    private void Update()
    {
        switch (gameState)
        {
            case GameState.WaitingGameplayInput:
                if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                {
                    MoveBlock(Vector2.left);
                }
                else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                {
                    MoveBlock(Vector2.right);
                }
                else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
                {
                    MoveBlock(Vector2.up);
                }
                else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
                {
                    MoveBlock(Vector2.down);
                }
                break;

            case GameState.LevelComplete:
                if (updateCompleteLevelsInFile)
                {
                    IncrementAndSaveCompletedLevels();
                }

                SetGameState(GameState.WaitingLevelCompleteInput);
                break;
        }
    }

    private Node GetNodeAtPosition(Vector2 position)
    {
        var possibleNodes = nodes
            .Where(n => !n.IsOccupied);

        if (!goal.GoalActive)
        {
            possibleNodes = possibleNodes
                .Where(n => !n.IsGoalNode);
        }

        return possibleNodes
            .FirstOrDefault(n => n.Position == position);
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
            next = GetNodeAtPosition(next.Position + direction);
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
