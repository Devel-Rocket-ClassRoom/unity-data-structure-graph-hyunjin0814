using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Stage stage;
    private Animator animator;
    private int currentTileId;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animator.speed = 0f;

        var findGo = GameObject.FindWithTag("Map");
        stage = findGo.GetComponent<Stage>();   
    }

    private void Update()
    {
        Sides direction = GetInputDirection();

        if (direction != Sides.None)
        {
            TryMove(direction);
        }
    }
    public void MoveTo(int tileId)
    {
        currentTileId = tileId;
        transform.position = stage.GetTilePos(currentTileId);
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
                stage.UpdateVisibility(tagetTile.id);
            }
        }
    }
}
    