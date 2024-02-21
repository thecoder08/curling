using OpenTK.Mathematics;

struct CollisionResult {
    public Vector3 velocity;
    public bool didCollide;
}

class Collision {
    public static CollisionResult Collide(Rock object1, Rock object2, float distance) {
        CollisionResult result = new CollisionResult();
        if (Vector3.Distance(object1.position, object2.position) <= distance) {
            result.didCollide = true;
            result.velocity = Vector3.Subtract(object1.velocity, Vector3.Multiply(Vector3.Subtract(object1.position, object2.position), 2*object2.mass/(object1.mass+object2.mass)*(Vector3.Dot(Vector3.Subtract(object1.velocity, object2.velocity), Vector3.Subtract(object1.position, object2.position))/Vector3.Subtract(object1.position, object2.position).LengthSquared)));
        }
        else {
            result.didCollide = false;
        }
        return result;
    }
}