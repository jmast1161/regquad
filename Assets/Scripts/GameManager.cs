using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;
using System.IO;

public enum GameState
{
    InitializeGame,
    InitializeLevel,
    WaitingGameplayInput,
    Moving,
    LevelComplete,
    WaitingLevelCompleteInput,
    GameOver,
    WaitingGameOverInput
}

public class GameManager : MonoBehaviour
{
    private GameState gameState;
    
    private List<Node> nodes;

    private Block player;

    private ICollection<Node> occupiedNodes;
    private ICollection<Target> targets;
    private ICollection<Bomb> bombs;
    private GameConfiguration gameConfiguration;
    private int remainingMoves;
    private Goal goal;
    private Canvas levelCompleteCanvas;
    private Canvas gameOverCanvas;
    private CurrentLevelIndex currentLevel;
    [SerializeField] private Node nodePrefab;
    [SerializeField] private Block blockPrefab;
    [SerializeField] private Target targetPrefab;
    [SerializeField] private Goal goalPrefab;
    [SerializeField] private Canvas levelCompletePrefab;
    [SerializeField] private Canvas remainingMovesCanvas;
    [SerializeField] private Canvas gameOverPrefab;
    [SerializeField] private Bomb bombPrefab;
    [SerializeField] private float travelTime = 0.5f;
    [SerializeField] private SoundManager soundManager;
    [SerializeField] private UnityEngine.UI.Button restartLevelButton;
    [SerializeField] private CurrentLevelIndex currentLevelIndexPrefab;

    // Start is called before the first frame update
    void Start()
    {
        SetGameState(GameState.InitializeGame);
        restartLevelButton.onClick.AddListener(RestartLevelButtonClicked);
    }

    private void SetGameState(GameState newState)
    {
        gameState = newState;

        switch(gameState)
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

    private void InitializeGame()
    {
        StreamReader reader = new StreamReader(Path.GetFullPath($@"{Application.dataPath}\Scripts\Levels.json"));
        string json = reader.ReadToEnd();
        gameConfiguration = JsonUtility.FromJson<GameConfiguration>(json);
        occupiedNodes = new List<Node>();
        nodes = new List<Node>();
        targets = new List<Target>();
        bombs = new List<Bomb>();
        soundManager.PlayMusicAudioSource();
        currentLevel = GameObject.FindObjectOfType<CurrentLevelIndex>();

        if (currentLevel == null)
        {
            currentLevel = Instantiate(currentLevelIndexPrefab);
        }

        if(currentLevel.CurrentLevel == 0)
        {
            currentLevel.CurrentLevel = 1;
        }

        SetGameState(GameState.InitializeLevel);
    }

    private void ClearLevelGameObjects()
    {
        if(occupiedNodes.Any())
        {
            foreach(var occupiedBlock in occupiedNodes)
            {
                Destroy(occupiedBlock.gameObject);
            }
            
            occupiedNodes.Clear();
        }

        if(bombs.Any())
        {
            foreach(var bomb in bombs)
            {
                Destroy(bomb.gameObject);
            }
            
            bombs.Clear();
        }

        if(nodes.Any())
        {
            foreach(var node in nodes)
            {
                Destroy(node.gameObject);
            }
            nodes.Clear();
        }

        if(targets.Any())
        {
            foreach(var target in targets)
            {
                Destroy(target.gameObject);
            }

            targets.Clear();
        }

        if(player != null)
        {
            Destroy(player.gameObject);
        }

        if(goal != null)
        {
            Destroy(goal.gameObject);
        }

        if(levelCompleteCanvas != null)
        {
            Destroy(levelCompleteCanvas.gameObject);
        }

        if(gameOverCanvas != null)
        {
            Destroy(gameOverCanvas.gameObject);
        }
    }

    private void InitializeLevel()
    {
        ClearLevelGameObjects();
        
        var level = gameConfiguration.Levels
            .FirstOrDefault(l => l.Index == currentLevel.CurrentLevel);

        if(level != null)
        {
            for(int x = 0; x < level.GridWidth; ++x){
                for(int y = 0; y < level.GridHeight; ++y){
                    var node = Instantiate(nodePrefab, new Vector2(x, y), Quaternion.identity);
                    nodes.Add(node);
                }
            }

            remainingMoves = level.Moves;
            AddOccupiedBlocks(level.OccupiedBlockLocations);
            SpawnBombs(level.BombLocations);
            SpawnPlayer(level.PlayerPosition);
            SpawnTargets(level.TargetLocations);
            SpawnGoal(level.GoalPosition);
            SpawnRemainingMoves();

            var center = new Vector2((float) level.GridWidth / 2 - 0.5f, (float) level.GridHeight / 2 - 0.5f);

            Camera.main.transform.position = new Vector3(center.x, center.y, -10);
            SetGameState(GameState.WaitingGameplayInput);
        }
    }

    private void AddOccupiedBlocks(Vector2[] occupiedBlockPositions)
    {   
        foreach(var occupiedBlockPosition in occupiedBlockPositions)
        {
            var node = nodes
                .Where(n => !n.IsGoalNode)
                .Where(n => n.Position.x == occupiedBlockPosition.x)
                .FirstOrDefault(n => n.Position.y == occupiedBlockPosition.y);
            if(node != null)
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
        levelCompleteCanvas = Instantiate(levelCompletePrefab, new Vector3(0f, 0f), Quaternion.identity);
        var nextLevelButton = levelCompleteCanvas.GetComponentInChildren<UnityEngine.UI.Button>();
        var entryAnimator = levelCompleteCanvas.GetComponentInChildren<Animator>();
        if(entryAnimator != null)
        {
            entryAnimator.Play("LevelCompletePanel", 0);
        }

        nextLevelButton.onClick.AddListener(NextLevelButtonClicked);
        SetGameState(GameState.WaitingLevelCompleteInput);
    }

    private void SpawnGameOverCanvas()
    {
        gameOverCanvas = Instantiate(gameOverPrefab, new Vector3(0f, 0f), Quaternion.identity);
        var replayButton = gameOverCanvas.GetComponentInChildren<UnityEngine.UI.Button>();
        var replayAnimator = gameOverCanvas.GetComponent<Animator>();
        if(replayAnimator != null)
        {
            replayAnimator.Play("LevelCompletePanel", 0);
        }

        replayButton.onClick.AddListener(() => 
        {
            SetGameState(GameState.InitializeLevel);
        });

        SetGameState(GameState.WaitingGameOverInput);
    }

    private void NextLevelButtonClicked()
    {
        ++currentLevel.CurrentLevel;
        SetGameState(GameState.InitializeLevel);
    }

    private void RestartLevelButtonClicked()
    {
        SetGameState(GameState.InitializeLevel);
    }

    private void SpawnBombs(Vector2[] bombLocations)
    {
        foreach(var bombLocation in bombLocations)
        {
            var node = nodes
                .Where(n => !n.IsGoalNode)
                .Where(n => !n.IsOccupied)
                .Where(n => n.Position.x == bombLocation.x)
                .FirstOrDefault(n => n.Position.y == bombLocation.y);

            if(node != null)
            {
                var bomb = Instantiate(bombPrefab, node.Position, Quaternion.identity);
                bomb.Init(node);
                bomb.ExplosionComplete += OnExplosionComplete;
                bombs.Add(bomb);
            }
        }
    }

    private void SpawnTargets(Vector2[] targetLocations)
    {        
        foreach(var targetLocation in targetLocations)
        {
            var node = nodes
                .Where(n => !n.IsGoalNode)
                .Where(n => !n.IsOccupied)
                .Where(n => n.Position.x == targetLocation.x)
                .FirstOrDefault(n => n.Position.y == targetLocation.y);

            if(node != null)
            {
                var target = Instantiate(targetPrefab,  node.Position, Quaternion.identity);
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

        if(node != null)
        {
            player = Instantiate(blockPrefab,  node.Position, Quaternion.identity);
            player.Init(node);
        }
    }

    private void SpawnRemainingMoves()
    {
        UpdateRemainingMoves(remainingMoves);
    }

    private void UpdateRemainingMoves(int moves)
    {
        if(remainingMovesCanvas == null)
        {
            return;
        }

        var text = remainingMovesCanvas.GetComponentInChildren<TMPro.TMP_Text>();
        if(text != null)
        {
            var baseText = text.text.Split(':');
            text.text = $"{baseText[0]}: {moves}";
        }
    }

    // Update is called once per frame
    private void Update()
    {
        switch(gameState)
        {
            case GameState.WaitingGameplayInput:
                if(Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    MoveBlock(Vector2.left);
                }
                else if(Input.GetKeyDown(KeyCode.RightArrow))
                {
                    MoveBlock(Vector2.right);
                }
                else if(Input.GetKeyDown(KeyCode.UpArrow))
                {
                    MoveBlock(Vector2.up);
                }
                else if(Input.GetKeyDown(KeyCode.DownArrow))
                {
                    MoveBlock(Vector2.down);
                }
                break;
        }
    }

    private Node GetNodeAtPosition(Vector2 position)
    {
        var possibleNodes = nodes
            .Where(n => !n.IsOccupied);

        if(!goal.GoalActive)
        {
            possibleNodes = possibleNodes
                .Where(n => !n.IsGoalNode);
        }

        return  possibleNodes
            .FirstOrDefault(n => n.Position == position);
    }

    private void MoveBlock(Vector2 direction)
    {
        SetGameState(GameState.Moving);

        var orderedNodes = nodes.OrderBy(x => x.Position.x).ThenBy(x => x.Position.y).ToList();
        if(direction == Vector2.right || direction == Vector2.up)
        {
            orderedNodes.Reverse();
        }

        var originalPosition = player.Node;
        var next = player.Node;
        var targetsToDelete = new List<Target>();
        var playerMovePath = new List<Vector3>();
        var bombHit = false;
        var blockDistance = 0;
        do{
            next = GetNodeAtPosition(next.Position + direction);
            ++blockDistance;
            if(next != null)
            {
                if(next.HasTarget)
                {
                    var target = targets.FirstOrDefault(t => t.Node == next);
                    targetsToDelete.Add(target);
                    soundManager.PlayTargetPickupSound(blockDistance);
                    next.HasTarget = false;
                }

                playerMovePath.Add(next.transform.position);
                player.Node = next;

                if(next.HasBomb)
                {
                    bombHit = true;
                    break;
                }
            }
        } 
        while(next != null);

        if(originalPosition != player.Node)
        {
            soundManager.PlayMoveSound();
            var sequence = DOTween.Sequence();
            sequence.Insert(0, player.transform.DOPath(playerMovePath.ToArray(), travelTime).SetEase(Ease.InQuad).OnWaypointChange((int index) => {
                if(index > 0)
                {
                    var previousNode = playerMovePath[index - 1];
                    var targetToDelete = targetsToDelete
                        .Where(x => x.Node.Position.x == previousNode.x)
                        .FirstOrDefault(x => x.Node.Position.y == previousNode.y);

                    if(targetToDelete != null)
                    {
                        targets.Remove(targetToDelete);
                        Destroy(targetToDelete.gameObject);
                    }
                }
            }));

            sequence.OnComplete(() => {
                if(playerMovePath.Count == 1 && targetsToDelete.Count == 1)
                {
                    targets.Remove(targetsToDelete[0]);
                    Destroy(targetsToDelete[0].gameObject);
                }

                if(!goal.GoalActive && !targets.Any())
                {
                    goal.SetGoalState(true);
                }

                UpdateRemainingMoves(--remainingMoves);
                if(player.Node.Position.x == goal.Node.Position.x &&
                    player.Node.Position.y == goal.Node.Position.y &&
                    goal.GoalActive)
                {
                    soundManager.PlayGoalSound();
                    SetGameState(GameState.LevelComplete);
                }
                else if(remainingMoves == 0)
                {
                    SetGameState(GameState.GameOver);
                }
                else if(bombHit)
                {
                    Destroy(player.gameObject);
                    
                    var bomb = bombs
                        .FirstOrDefault(b => b.Node == next);
                    
                    if(bomb != null)
                    {
                        soundManager.PlayExplosionSound();
                        bomb.PlayExplosion();
                    }

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
        bombs.Remove(bomb);
        Destroy(bomb.gameObject);
        SetGameState(GameState.GameOver);
    }
}
