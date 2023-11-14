using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private Node nodePrefab;
    [SerializeField] private Block blockPrefab;
    [SerializeField] private Target targetPrefab;
    [SerializeField] private Goal goalPrefab;
    [SerializeField] private StopBlock stopBlockPrefab;

    [SerializeField] private Bomb bombPrefab;
    [SerializeField] private ExplodeBombBlock explodeBombBlockPrefab;
    [SerializeField] private DirectionBlock directionBlockPrefab;
    [SerializeField] private TextAsset configuration;

    public ICollection<Node> AddOccupiedBlocks(Vector2[] occupiedBlockPositions, List<Node> nodes)
    {
        var occupiedNodes = new List<Node>();
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

        return occupiedNodes;
    }

    public Goal SpawnGoal(Vector2 goalPosition, List<Node> nodes, bool goalState)
    {
        var node = nodes
            .Where(n => !n.IsGoalNode)
            .Where(n => n.Position.x == goalPosition.x)
            .FirstOrDefault(n => n.Position.y == goalPosition.y);
        var goal = Instantiate(goalPrefab, goalPosition, Quaternion.identity);
        goal.Init(node);
        goal.SetGoalState(goalState);
        return goal;
    }


    public List<StopBlock> SpawnStopBlocks(Vector2[] stopBlockPositions, List<Node> nodes)
    {
        var stopBlocks = new List<StopBlock>();
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

        return stopBlocks;
    }

    public List<DirectionBlock> SpawnDirectionBlocks(DirectionBlockConfig[] directionBlockConfigs, List<Node> nodes)
    {
        var directionBlocks = new List<DirectionBlock>();
        if (directionBlockConfigs == null)
        {
            return directionBlocks;
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

        return directionBlocks;
    }


    public List<Bomb> SpawnBombs(Vector2[] bombLocations, List<Node> nodes)
    {
        var bombs = new List<Bomb>();
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
                bombs.Add(bomb);
            }
        }

        return bombs;
    }

    public List<ExplodeBombBlock> SpawnExplodeBombBlocks(Vector2[] explodeBombBlockLocations, List<Node> nodes)
    {
        var explodeBombBlocks = new List<ExplodeBombBlock>();
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

        return explodeBombBlocks;
    }

    public List<Target> SpawnTargets(Vector2[] targetLocations, List<Node> nodes)
    {
        var targets = new List<Target>();
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

        return targets;
    }

    public Block SpawnPlayer(Vector2 position, List<Node> nodes)
    {
        var node = nodes
            .Where(n => !n.IsGoalNode)
            .Where(n => !n.IsOccupied)
            .Where(n => n.Position.x == position.x)
            .FirstOrDefault(n => n.Position.y == position.y);

        if (node != null)
        {
            var player = Instantiate(blockPrefab, node.Position, Quaternion.identity);
            player.Init(node);

            return player;
        }

        return null;
    }


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
