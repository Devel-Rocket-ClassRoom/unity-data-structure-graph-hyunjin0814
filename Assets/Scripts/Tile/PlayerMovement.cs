using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Stage stage;
    private Animator animator;
    private int currentTileId = -1;
    private int targetTileId = -1;

    public int CurrentTileId => currentTileId;

    private bool isMoving = false;
    public float moveSpeed = 10f;
    private Coroutine moveCoroutine = null;

    //private int count = 0;

    public bool IsMoving => isMoving;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animator.speed = 0f;

        var findGo = GameObject.FindWithTag("Map");
        stage = findGo.GetComponent<Stage>();   
    }

    private void Update()
    {
        if (isMoving) 
            return;

        Sides direction = GetInputDirection();

        if (direction != Sides.None)
        {
            TryMove(direction);
        }
    }
    public void MoveTo(int tileId)
    {
        targetTileId = tileId;

        if (moveCoroutine != null)
        {
            ClearCoroutine();
        }

        moveCoroutine = StartCoroutine(MoveRoutine());
    }

    private IEnumerator MoveRoutine()
    {
        isMoving = true;
        animator.speed = 1f;
        var startPos = transform.position;
        var targetPos = stage.GetTilePos(targetTileId); 
        var duration = Vector3.Distance(startPos, targetPos) / moveSpeed;

        float elapsedTime = 0f;
        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime / duration;
            transform.position = Vector3.Lerp(startPos, targetPos, elapsedTime);
            yield return null;
        }

        transform.position = targetPos;
        animator.speed = 0f;

        currentTileId = targetTileId;
        targetTileId = -1;
        stage.OnTileVisited(currentTileId);
        //stage.UpdateVisibility(currentTileId);
        moveCoroutine = null;
        isMoving = false;
    }

    private Sides GetInputDirection()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow)) return Sides.Top;
        if (Input.GetKeyDown(KeyCode.DownArrow)) return Sides.Bottom;
        if (Input.GetKeyDown(KeyCode.LeftArrow)) return Sides.Left;
        if (Input.GetKeyDown(KeyCode.RightArrow)) return Sides.Right;

        return Sides.None;
    }

    private void TryMove(Sides direction)
    {
        if (direction != Sides.None)
        {
            var tagetTile = stage.Map.tiles[currentTileId].adjacents[(int)direction];
            if (tagetTile != null && tagetTile.CanMove)
            {
                MoveTo(tagetTile.id);
            }
        }
    }

    public void SetPosition(int tileId)
    {
        currentTileId = tileId;
        targetTileId = -1;

        transform.position = stage.GetTilePos(tileId);

        if (moveCoroutine != null)
        {
            ClearCoroutine();
        }
    }

    private void ClearCoroutine()
    {
        StopCoroutine(moveCoroutine);
        moveCoroutine = null;
        animator.speed = 0f;
        isMoving = false;
    }

    public void FollowPath(List<Tile> path)
    {
        if (moveCoroutine != null)
        {
            ClearCoroutine();
        }

        moveCoroutine = StartCoroutine(PathMoveRoutine(path));
    }

    private IEnumerator PathMoveRoutine(List<Tile> path)
    {
        isMoving = true;
        animator.speed = 1f;
        //count = 0;

        foreach (var tile in path)
        {
            targetTileId = tile.id;
            Vector3 targetPos = stage.GetTilePos(targetTileId);

            while (Vector3.Distance(transform.position, targetPos) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    targetPos,
                    moveSpeed * Time.deltaTime
                );
                yield return null;
            }

            transform.position = targetPos;
            currentTileId = targetTileId;

            stage.OnTileVisited(currentTileId);
            //count++;
        }

        animator.speed = 0f;
        moveCoroutine = null;
        isMoving = false;
        //Debug.Log($"타일 {count - 1}개 이동");
    }
}
    