using OpenTK.Mathematics;

struct CollisionResult {
    public Vector2 velocity1;
    public Vector2 velocity2;
}

class Collision {
    public static CollisionResult Collide(Vector2 position1, Vector2 velocity1, float mass1, Vector2 position2, Vector2 velocity2, float mass2) {
        CollisionResult result;
        result.velocity1 = Vector2.Subtract(velocity1, Vector2.Multiply(Vector2.Subtract(position1, position2), 2*mass2/(mass1+mass2)*(Vector2.Dot(Vector2.Subtract(velocity1, velocity2), Vector2.Subtract(position1, position2))/Vector2.Subtract(position1, position2).LengthSquared)));
        result.velocity2 = Vector2.Subtract(velocity2, Vector2.Multiply(Vector2.Subtract(position2, position1), 2*mass1/(mass1+mass2)*(Vector2.Dot(Vector2.Subtract(velocity2, velocity1), Vector2.Subtract(position2, position1))/Vector2.Subtract(position2, position1).LengthSquared)));
        return result;
    }
    public static bool HaveCollided(Vector2 object1, Vector2 object2, float distance) {
        return Vector2.Distance(object1, object2) < distance;
    }
}