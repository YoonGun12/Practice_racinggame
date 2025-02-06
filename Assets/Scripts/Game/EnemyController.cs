using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float speed = 5f; // 적 이동 속도
    private bool canMove = false;
    private float moveDirection;

    void Update()
    {
        if (canMove)
        {
            transform.Translate(Vector3.back * (speed * Time.deltaTime));
            MoveHorizontally();
        }

        // 화면 아래로 벗어나면 비활성화
        if (transform.position.z < -10f)
        {
            GameManager gm = FindObjectOfType<GameManager>();
            gm.ReturnEnemyToPool(gameObject);
        }
    }

    public void ActivateEnemy(float elapsedTime)
    {
        canMove = false;
        transform.position = new Vector3(Random.Range(-1.5f, 1.5f), 0, transform.position.z);

        if (elapsedTime >= 30f)
        {
            canMove = true;
            StartCoroutine(RandomHorizontalMovement());
        }
        else
        {
            Invoke(nameof(EnableMovement), 5f); // 30초 전에는 5초 대기 후 이동
        }
    }

    void EnableMovement()
    {
        canMove = true;
    }

    IEnumerator RandomHorizontalMovement()
    {
        while (canMove)
        {
            moveDirection = Random.Range(-1f, 1f);
            yield return new WaitForSeconds(0.5f);
        }
    }

    void MoveHorizontally()
    {
        if (moveDirection != 0)
        {
            float newX = Mathf.Clamp(transform.position.x + moveDirection * Time.deltaTime, -1.5f, 1.5f);
            transform.position = new Vector3(newX, transform.position.y, transform.position.z);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager gm = FindObjectOfType<GameManager>();
            gm.GameOver();
        }
    }
}
