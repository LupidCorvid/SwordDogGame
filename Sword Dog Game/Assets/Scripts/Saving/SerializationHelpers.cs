using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerSerialization
{
    public ControllerSerialization controller;
    // public InventorySerialization inventory;
    // public HealthSerialization health;
    // public AttackSerialization attack;
    public SpawnpointSerialization spawnpoint;

    public PlayerSerialization(GameObject playerObj)
    {
        controller = new ControllerSerialization(playerObj.GetComponent<PlayerMovement>());
        // inventory = new InventorySerialization(playerObj.GetComponent<Inventory>());
        // health = new HealthSerialization(playerObj.GetComponent<Health>());
        // attack = new AttackSerialization(playerObj.GetComponent<Attack>());
        spawnpoint = new SpawnpointSerialization(playerObj.GetComponent<Spawnpoint>());
    }

}

[Serializable]
public class ControllerSerialization
{
    public bool isGrounded, invincible, isJumping;

    public ControllerSerialization(PlayerMovement controller)
    {
        isGrounded = controller.isGrounded;
        invincible = controller.invincible;
        isJumping = controller.isJumping;
    }

    public void SetValues(GameObject playerObj)
    {
        playerObj.GetComponent<PlayerMovement>().isGrounded = isGrounded;
        playerObj.GetComponent<PlayerMovement>().invincible = invincible;
        playerObj.GetComponent<PlayerMovement>().isJumping = isJumping;
    }
}

// [Serializable]
// public class HealthSerialization
// {
//     public int playerHealth, numberOfHearts;

//     public HealthSerialization(health health)
//     {
//         playerHealth = health.playerHealth;
//         numberOfHearts = health.numberOfHearts;
//     }

//     public void SetValues(GameObject playerObj)
//     {
//         playerObj.GetComponent<health>().numberOfHearts = numberOfHearts;
//         playerObj.GetComponent<health>().playerHealth = numberOfHearts;
//     }
// }

// [Serializable]
// public class AttackSerialization
// {
//     public float dmgValue, specialCooldown, specialMaxCooldown;
//     public bool shooting_Unlocked;

//     public AttackSerialization(Attack attack)
//     {
//         dmgValue = attack.dmgValue;
//         shooting_Unlocked = attack.shooting_Unlocked;
//         specialCooldown = attack.specialCooldown;
//         specialMaxCooldown = attack.specialMaxCooldown;
//     }

//     public void SetValues(GameObject playerObj)
//     {
//         playerObj.GetComponent<Attack>().dmgValue = dmgValue;
//         playerObj.GetComponent<Attack>().shooting_Unlocked = shooting_Unlocked;
//         playerObj.GetComponent<Attack>().specialCooldown = specialCooldown;
//         playerObj.GetComponent<Attack>().specialMaxCooldown = specialMaxCooldown;
//     }
// }

[Serializable]
public class SpawnpointSerialization
{
    public string scene;
    public Vector2 position;

    public SpawnpointSerialization(Spawnpoint spawnpoint)
    {
        scene = spawnpoint.scene;
        position = spawnpoint.position;
    }

    public void SetValues(GameObject playerObj)
    {
        playerObj.GetComponent<Spawnpoint>().scene = scene;
        playerObj.GetComponent<Spawnpoint>().position = position;
    }
}

[Serializable]
public class Vector3Serialization
{
    public float x, y, z;

    public Vector3Serialization(Vector3 position)
    {
        x = position.x;
        y = position.y;
        z = position.z;
    }

    public Vector3 GetValue()
    {
        return new Vector3(x, y, z);
    }
}

[Serializable]
public class Vector2Serialization
{
    public float x, y;

    public Vector2Serialization(Vector2 position)
    {
        x = position.x;
        y = position.y;
    }

    public Vector2 GetValue()
    {
        return new Vector2(x, y);
    }
}

[Serializable]
public class OptionsSerialization
{
    public bool musicMute;
    public float musicVolume;

    public OptionsSerialization(AudioManager am)
    {
        musicMute = am.mute;
        musicVolume = am.volume;
    }

    public void SetValues()
    {
        AudioManager.instance.mute = musicMute;
        AudioManager.instance.volume = musicVolume;
    }
}