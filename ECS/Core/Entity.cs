using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Entity : IEquatable<Entity> {
    private int id;

    public int Id { get { return id; } }
    public Entity(int id) {
        this.id = id;
    }
    
    public override int GetHashCode() {
        return id;
    }
    public override bool Equals(object obj) {
        return obj is Entity && ((Entity)obj).id == id;
    }

    public bool Equals(Entity other) {
        return id == other.id;
    }
}
