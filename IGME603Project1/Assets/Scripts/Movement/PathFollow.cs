using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PathFollow : MonoBehaviour
{
    private List<Vector3> positions = new List<Vector3>();
    private int targetPosIndex = 0;

    [Tooltip("Defines the moving object's behavior when it reaches the end of the path. If true, the object will reverse its path. If false, the object will loop back to its first position.")]
    [SerializeField] bool shouldBacktrack = false;
    private bool isBacktracking = false;
    [SerializeField] private float speed = 3;

    Rigidbody2D _rigidbody;

    private List<Vector3> GetPathPositions(string childName)
    {
        List<Vector3> pathPositions = new List<Vector3>();
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.name.StartsWith("Path Point")) //handle duplicate names like "Path Point (1)"
            {
                pathPositions.Add(child.transform.position);
            }
        }
        return pathPositions;
    }

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _rigidbody.bodyType = RigidbodyType2D.Kinematic;

        positions = GetPathPositions("Path Point");
        if (positions.Count == 0)
        {
            Debug.LogWarning("An object with PathFollow.cs attached does not have a path set! Define the path by adding child GameObjects named \"Path Point\".");
        }
    }
    
    // Update is called once per frame
    private void FixedUpdate()
    {
        if (positions.Count > 0)
        {
            //If at the target point, find the next point to go to
            if (transform.position == positions[targetPosIndex])
            {
                //If going backwards...
                if (isBacktracking)
                {
                    //If at the first point, start going forwards again
                    if (targetPosIndex - 1 < 0)
                    {
                        isBacktracking = false;
                        targetPosIndex++;
                    }
                    //Otherwise go to the previous point
                    else
                    {
                        targetPosIndex--;
                    }
                }
                //Otherwise (going forwards), if at the last point...
                else if (targetPosIndex + 1 >= positions.Count)
                {
                    //If backtracking is enabled, start going backwards
                    if (shouldBacktrack)
                    {
                        isBacktracking = true;
                        targetPosIndex--;
                    }
                    //Otherwise loop to the first point
                    else
                    {
                        targetPosIndex = 0;
                    }
                }
                //Otherwise go to the next point
                else
                {
                    targetPosIndex++;
                }
            }

            Vector3 newPosition = Vector3.MoveTowards(
                transform.position,
                positions[targetPosIndex],
                speed * Time.deltaTime
            );
            _rigidbody.MovePosition(newPosition);
        }
    }

    private void OnDrawGizmosSelected()
    {
        List<Vector3> editorPositions = GetPathPositions("Path Point");
        if (editorPositions.Count > 1)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < editorPositions.Count - 1; i++)
            {
                Gizmos.DrawLine(editorPositions[i], editorPositions[i + 1]);
            }
            if (!shouldBacktrack)
            {
                Gizmos.DrawLine(editorPositions[editorPositions.Count - 1], editorPositions[0]);
            }
        }
    }
}