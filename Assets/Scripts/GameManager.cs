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
    WaitingInput,
    Moving,
    LevelComplete

}

public class GameManager : MonoBehaviour
{
    private GameState gameState;
    
    private List<Node> nodes;

    private Block player;

    private ICollection<Block> occupiedBlocks;
    private ICollection<Target> targets;
    private GameConfiguration gameConfiguration;
    private int currentLevelIndex;
    private Goal goal;
    private SpriteRenderer board;
    [SerializeField] private Node nodePrefab;
    [SerializeField] private Block blockPrefab;
    [SerializeField] private Target targetPrefab;
    [SerializeField] private Goal goalPrefab;
    [SerializeField] private SpriteRenderer boardPrefab;
    [SerializeField] private float travelTime = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        SetGameState(GameState.InitializeGame);
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
            case GameState.WaitingInput:
                break;
            case GameState.Moving:
                break;
        }
    }

    private void InitializeGame()
    {
        StreamReader reader = new StreamReader(Path.GetFullPath($@"{Application.dataPath}\Scripts\Levels.json"));
        string json = reader.ReadToEnd();
        gameConfiguration = JsonUtility.FromJson<GameConfiguration>(json);
        currentLevelIndex = 1;
        occupiedBlocks = new List<Block>();
        nodes = new List<Node>();
        targets = new List<Target>();

        SetGameState(GameState.InitializeLevel);
    }

    private void InitializeLevel()
    {
        if(occupiedBlocks.Any())
        {
            foreach(var occupiedBlock in occupiedBlocks)
            {
                Destroy(occupiedBlock.gameObject);
            }
            
            occupiedBlocks.Clear();
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

        if(board != null)
        {
            Destroy(board.gameObject);
        }

        var level = gameConfiguration.Levels
            .FirstOrDefault(l => l.Index == currentLevelIndex);

        if(level != null)
        {
            for(int x = 0; x < level.GridWidth; ++x){
                for(int y = 0; y < level.GridHeight; ++y){
                    var node = Instantiate(nodePrefab, new Vector2(x, y), Quaternion.identity);
                    nodes.Add(node);
                }
            }

            AddOccupiedBlocks(level.OccupiedBlockLocations);
            SpawnPlayer(level.PlayerPosition);
            SpawnTargets(level.TargetLocations);
            SpawnGoal(level.GoalPosition);

            var center = new Vector2((float) level.GridWidth / 2 - 0.5f, (float) level.GridHeight / 2 - 0.5f);

            board = Instantiate(boardPrefab, center, Quaternion.identity);
            board.size = new Vector2(level.GridWidth, level.GridHeight);

            Camera.main.transform.position = new Vector3(center.x, center.y, -10);
            SetGameState(GameState.WaitingInput);
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
                var occupiedBlock = Instantiate(blockPrefab,  node.Position, Quaternion.identity);
                occupiedBlock.Init(node, Color.red, false);

                occupiedBlocks.Add(occupiedBlock);
            }
        }
    }

    private void SpawnGoal(Vector2 goalPosition)
    {
        var node = Instantiate(nodePrefab, goalPosition, Quaternion.identity);
        node.IsGoalNode = true;
        nodes.Add(node);
        goal = Instantiate(goalPrefab, goalPosition, Quaternion.identity);
        goal.Init(node);
        goal.SetGoalState(!targets.Any());
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
            player.Init(node, Color.white, true);
        }
    }

    // Update is called once per frame
    private void Update()
    {
        switch(gameState)
        {
            case GameState.WaitingInput:
                if(Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    MoveBlock(Vector2.left);
                }

                if(Input.GetKeyDown(KeyCode.RightArrow))
                {
                    MoveBlock(Vector2.right);
                }

                if(Input.GetKeyDown(KeyCode.UpArrow))
                {
                    MoveBlock(Vector2.up);
                }

                if(Input.GetKeyDown(KeyCode.DownArrow))
                {
                    MoveBlock(Vector2.down);
                }
                break;

            case GameState.LevelComplete:

                if(Input.GetKeyDown(KeyCode.Space))
                {
                    ++currentLevelIndex;
                    SetGameState(GameState.InitializeLevel);
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
        do{
            next = GetNodeAtPosition(next.Position + direction);
            if(next != null)
            {
                if(next.HasTarget)
                {
                    var target = targets.FirstOrDefault(t => t.Node == next);
                    targetsToDelete.Add(target);
                    next.HasTarget = false;
                }
                playerMovePath.Add(next.transform.position);
                player.Node = next;
            }
        } 
        while(next != null);

        if(originalPosition != player.Node)
        {
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

                if(player.Node == goal.Node)
                {
                    SetGameState(GameState.LevelComplete);
                }
                else
                {
                    SetGameState(GameState.WaitingInput);
                }
            });
        }
        else
        {
            SetGameState(GameState.WaitingInput);
        }
    }
}
