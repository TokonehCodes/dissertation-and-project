using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : Adjecency
{

    Rigidbody2D opp;
    Path testPath;
    [SerializeField] float speed = 3f;
    [SerializeField] float jumpSpeed = 8f;
    int myCount;

    // Start is called before the first frame update
    void Start()
    {
        myCount = 0;
        opp = GetComponent<Rigidbody2D>();        
    }


    //Update is called once per frame
    void Update()
    {
        if(testPath != null)followPath(testPath);
    }

    public void GetPath(Path p)
    {
        testPath = p;
    }
    public void followPath(Path p){
        if(p.platforms.Count > myCount + 1){
            moveToNext(p.platforms[myCount], p.platforms[myCount+1]);
        }
        else {
            opp.velocity = new Vector2(0, opp.velocity.y); 
            myCount = 0;
        }      
    }

    void moveToNext(string current, string next){

        CapsuleCollider2D currentColl = GameObject.Find(current).GetComponent<CapsuleCollider2D>();
        CapsuleCollider2D nextColl = GameObject.Find(next).GetComponent<CapsuleCollider2D>();

        if(Adjecency.state[current].x > Adjecency.state[next].x){ //next is to the left
            Vector2 direction = new Vector2(nextColl.bounds.max.x, state[next].y) - new Vector2(currentColl.bounds.min.x, state[current].y);            
            float sign = (direction.x >= 0) ? 1 : -1;
            float offset = (sign >= 0) ? 0 :360;
            float angle = Vector2.Angle(Vector2.up, direction) * sign + offset;
            
            if(angle > 90 && angle <= 210){
                if(!opp.IsTouching(nextColl)){
                    opp.velocity = new Vector2(-speed, opp.velocity.y);
                }
                else{
                    myCount++;
                    opp.velocity = new Vector2(0, opp.velocity.y);
                }
            }

            else {
                Vector2 dest = new Vector2();
                if(angle <= 90){
                    dest = new Vector2(nextColl.bounds.max.x, Adjecency.state[current].y);
                }
                else if(angle >= 210){
                    dest = new Vector2(currentColl.bounds.min.x, Adjecency.state[current].y);
                }
                if(transform.position.x > dest.x + 1f){
                    opp.velocity = new Vector2(-speed, opp.velocity.y);
                }
                else if (!(transform.position.x > dest.x + 1f)){
                    opp.velocity = new Vector2 (0, opp.velocity.y);
                    jump();
                    if(transform.position.y > Adjecency.state[next].y){
                        if(transform.position.x > Adjecency.state[next].x){
                            opp.velocity = new Vector2(-speed, opp.velocity.y);
                        }
                        else{
                            myCount++;
                            opp.velocity = new Vector2(0, opp.velocity.y);
                        }
                    }
                }
            }
        }

        if(Adjecency.state[current].x < Adjecency.state[next].x){ //next is to the right
            Vector2 direction = new Vector2(nextColl.bounds.min.x, state[next].y) - new Vector2(currentColl.bounds.max.x, state[current].y);
            float sign = (direction.x >= 0) ? 1 : -1;
            float offset = (sign >= 0) ? 0 :360;
            float angle = Vector2.Angle(Vector2.up, direction) * sign + offset;

            if(angle > 150 && angle < 270){
                if(!opp.IsTouching(nextColl)){
                    opp.velocity = new Vector2(speed, opp.velocity.y);
                }
                else{
                    myCount++;
                    opp.velocity = new Vector2(0, opp.velocity.y);
                }
            }

            else{
                Vector2 dest = new Vector2();
                if(angle > 270){
                    dest = new Vector2(nextColl.bounds.min.x, Adjecency.state[current].y);
                }
                else if(angle > 0 && angle <= 150){
                    dest = new Vector2(currentColl.bounds.max.x, Adjecency.state[current].y);
                }
                if(transform.position.x < dest.x - 1f){
                    opp.velocity = new Vector2(speed, opp.velocity.y);
                }
                else if(!(transform.position.x < dest.x - 1f)){
                    opp.velocity = new Vector2 (0, opp.velocity.y);
                    jump();
                    if(transform.position.y > Adjecency.state[next].y - 1f){
                        if(transform.position.x < Adjecency.state[next].x){
                            opp.velocity = new Vector2(speed, opp.velocity.y);
                        }
                        else{
                            myCount++;
                            opp.velocity = new Vector2(0, opp.velocity.y);
                        }
                    }
                }
            }
        }
    }

    void jump(){
        bool grounded = Physics2D.Raycast(transform.position, Vector2.down, 0.6f);
        if (grounded){
            opp.velocity = Vector2.up * jumpSpeed;
        }
    }

}







