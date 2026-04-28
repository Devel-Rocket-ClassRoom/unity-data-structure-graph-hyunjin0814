using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Stage stage;
    private Animator animator;
    private int currentTileId;

    public float moveDuration = 0.5f;

    private Coroutine moveCoroutine;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animator.speed = 0f;

        var findGo = GameObject.FindWithTag("Map");
        stage = findGo.GetComponent<Stage>();   
    }

    private void Update()
    {
        if (moveCoroutine != null) 
            return;

        Sides direction = GetInputDirection();

        if (direction != Sides.None)
        {
            TryMove(direction);
        }
    }
    public void MoveTo(int tileId)
    {
        currentTileId = tileId;

        if (moveCoroutine != null)
        {
            ClearCoroutine();
        }

        moveCoroutine = StartCoroutine(MoveRoutine());
    }

    private IEnumerator MoveRoutine()
    {
        var startPos = transform.position;
        var targetPos = stage.GetTilePos(currentTileId); 
        float elapsedTime = 0f;

        animator.speed = 1f;
        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / moveDuration;

            transform.position = Vector3.Lerp(startPos, targetPos, t);

            yield return null;
        }

        transform.position = targetPos;

        stage.UpdateVisibility(currentTileId);

        moveCoroutine = null;
        animator.speed = 0f;
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
    }
}
    