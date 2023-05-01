using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;
using System.IO;

public enum GameState
{
    InitializeLevel,
    WaitingInput,
    Moving

}

public class GameManager : MonoBehaviour
{
    private GameState gameState;
    
    private List<Node> nodes;

    private Block player;

    private ICollection<Block> occupiedBlocks;
    private ICollection<Target> targets;

    private int CurrentLevelIndex = 1;

    [SerializeField] private int width = 4;
    [SerializeField] private int height = 4;
    [SerializeField] private Node nodePrefab;
    [SerializeField] private Block blockPrefab;
    [SerializeField] private Target targetPrefab;
    [SerializeField] private SpriteRenderer boardPrefab;
    [SerializeField] private float travelTime = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        SetGameState(GameState.InitializeLevel);
    }

    private void SetGameState(GameState newState)
    {
        gameState = newState;

        switch(gameState)
        {
            case GameState.InitializeLevel:
                InitializeLevel();
                break;
            case GameState.WaitingInput:
                break;
            case GameState.Moving:
                break;
        }
    }

    private void InitializeLevel()
    {
        //todo: get relative path
        StreamReader reader = new StreamReader(Path.GetFullPath(@"D:\SoftwareDevelopment\GameDevelopment\ExitGame\Assets\Scripts\Levels.json"));
        string json = reader.ReadToEnd();
        var gameConfiguration = JsonUtility.FromJson<GameConfiguration>(json);

        occupiedBlocks = new List<Block>();
        nodes = new List<Node>();
        targets = new List<Target>();
        
        var level = gameConfiguration.Levels
            .FirstOrDefault(l => l.Index == CurrentLevelIndex);

        if(level != null)
        {
            for(int x = 0; x < level.GridWidth; ++x){
                for(int y = 0; y < level.GridHeight; ++y){
                    var node = Instantiate(nodePrefab, new Vector2(x, y), Quaternion.identity);
                    nodes.Add(node);
                }
            }            

            AddOccupiedBlocks(level.OccupiedBlockLocations);
            ++CurrentLevelIndex;
        }

        var center = new Vector2((float) width / 2 - 0.5f, (float) height / 2 - 0.5f);

        var board = Instantiate(boardPrefab, center, Quaternion.identity);
        board.size = new Vector2(width, height);

        Camera.main.transform.position = new Vector3(center.x, center.y, -10);
        SpawnPlayer();
        SpawnTargets();
        SetGameState(GameState.WaitingInput);        
    }

    private void AddOccupiedBlocks(Vector2[] occupiedBlockPositions)
    {
        // var randomIndex = Random.Range(0, nodes.Count - 1);
        // var node = nodes[randomIndex];        
        foreach(var occupiedBlockPosition in occupiedBlockPositions)
        {
            var node = nodes
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

    private void SpawnTargets()
    {        
        var node = nodes
            .Where(n => !n.IsOccupied)
            .Where(n => n.Position.x == 1)
            .FirstOrDefault(n => n.Position.y == 0);

         if(node != null)
        {
            var target = Instantiate(targetPrefab,  node.Position, Quaternion.identity);
            target.Init(node);
            targets.Add(target);
        }
    }

    private void SpawnPlayer()
    {
        var node = nodes
            .Where(n => !n.IsOccupied)
            .Where(n => n.Position.x == 1)
            .FirstOrDefault(n => n.Position.y == 1);

        if(node != null)
        {
            player = Instantiate(blockPrefab,  node.Position, Quaternion.identity);
            player.Init(node, Color.white, true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(gameState != GameState.WaitingInput)
        {
            return;
        }

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
    }

    private Node GetNodeAtPosition(Vector2 position) =>
        nodes
        .Where(n => !n.IsOccupied)
        .FirstOrDefault(n => n.Position == position);    

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

                player.Node = next;
            }
        } 
        while(next != null);

        if(originalPosition != player.Node)
        {
            var sequence = DOTween.Sequence();        
            sequence.Insert(0, player.transform.DOMove(player.Node.Position, travelTime).SetEase(Ease.InQuad));
            sequence.OnUpdate(() => {
                
            });
            sequence.OnComplete(() => {
               foreach(var target in targetsToDelete)
               {
                    targets.Remove(target);
                    Destroy(target.gameObject);
               }

                SetGameState(GameState.WaitingInput);
            });
        }
        else
        {
            SetGameState(GameState.WaitingInput);
        }
    }
}
