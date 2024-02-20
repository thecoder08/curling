using OpenTK.Mathematics;

struct CollisionResult {
    public Vector3 velocity1;
    public Vector3 velocity2;
    public bool didCollide;
}

class Collision {
    public static CollisionResult Collide(Rock object1, Rock object2, float distance) {
        CollisionResult result = new CollisionResult();
        if (Vector3.Distance(object1.position, object2.position) < distance) {
            result.didCollide = true;
            result.velocity1 = Vector3.Subtract(object1.velocity, Vector3.Multiply(Vector3.Subtract(object1.position, object2.position), 2*object2.mass/(object1.mass+object2.mass)*(Vector3.Dot(Vector3.Subtract(object1.velocity, object2.velocity), Vector3.Subtract(object1.position, object2.position))/Vector3.Subtract(object1.position, object2.position).LengthSquared)));
            result.velocity2 = Vector3.Subtract(object2.velocity, Vector3.Multiply(Vector3.Subtract(object2.position, object1.position), 2*object1.mass/(object1.mass+object2.mass)*(Vector3.Dot(Vector3.Subtract(object2.velocity, object1.velocity), Vector3.Subtract(object2.position, object1.position))/Vector3.Subtract(object2.position, object1.position).LengthSquared)));
        }
        else {
            result.didCollide = false;
        }
        return result;
    }
}