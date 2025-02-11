# Multiplayer Endless Runner
A multiplayer endless runner game featuring a **local player on the right side** and a **network-synced remote player on the left side**. The game is designed for **minimal network traffic** by **sending only movement commands instead of full position updates**.

![Unity Multiplayer](https://img.shields.io/badge/Multiplayer-Netcode-blue.svg)

---

## 📌 Table of Contents
- [Overview](#overview)
- [Features](#features)
- [Optimization Techniques](#optimization-techniques)
- [Multiplayer Implementation](#multiplayer-implementation)
- [Game Flow & Interaction](#game-flow--interaction)
- [Debugging & Network Efficiency](#debugging--network-efficiency)
- [Installation & Running](#installation--running)
- [Future Improvements](#future-improvements)

---

## **Overview**
This project is a **mobile-friendly endless runner** with an integrated **multiplayer system** using **Unity Netcode** and **Unity Relay**. It is optimized to **minimize data transmission** while ensuring **smooth and responsive gameplay**.

### **How It Works**
- The **right half of the screen** is controlled by the local player.
- The **left half mirrors a network-synced player** with efficient data synchronization.
- Instead of transmitting **position data every frame**, we **send only movement commands** and handle position updates **locally** on each client.

---

## **Features**
✔️ **Automatic Running** – The player moves forward continuously.  
✔️ **Swipe-Based Movement** – Left, right, and jump via swipe gestures.  
✔️ **Remote Player Synchronization** – The left-side player mirrors movements using **minimal data transmission**.  
✔️ **Dynamic Obstacle & Coin Spawning** – Procedurally placed collectibles and obstacles.  
✔️ **Optimized Multiplayer Communication** – Instead of sending **position updates**, only **lane changes** and **jump actions** are sent via RPC.  
✔️ **Network-Based Game Start** – The game **only starts when both players are connected**.  
✔️ **Independent Game Over Handling** – If one player dies, the other can continue playing.  
✔️ **Performance Optimizations** – Smooth gameplay on **mobile devices** with **low CPU/memory usage**.  
✔️ **Debug Logging** – Tracks **data transmission size** and **sync accuracy** for optimization.  

---

## **Optimization Techniques**
### **1️⃣ Sending Movement Commands Instead of Position Updates**
Most multiplayer games **continuously synchronize player positions**, resulting in **high network bandwidth usage**. Instead, we:
- **Send only movement commands** (`lane change` or `jump`) via RPC.
- **Handle actual movement locally** on each client.
- **Let Z-axis movement run independently**, as both players move forward automatically.

| Approach  | Data Sent (Bytes) | Frequency | Total Bandwidth Usage |
|-----------|------------------|-----------|-----------------------|
| **Sending Position Every Frame** | `sizeof(Vector3) = 12B` | 60 FPS | **~720B per second** |
| **Sending Lane Changes & Jumps Only (Optimized)** | `sizeof(int) + sizeof(bool) = 5B` | Only on input | **~5B per input event** |

🔹 **Our approach reduces bandwidth usage by up to ~99.3%!**  
🔹 **Results in smoother gameplay even with high latency networks.**  

### **2️⃣ Predicting & Applying Movement Locally**
Instead of waiting for network updates:
- Each client **predicts player movement** based on known rules.
- Lane switching and jumping **are handled locally** to reduce lag.
- This ensures **immediate response time**, improving player experience.

### **3️⃣ Procedural Synchronization of Obstacles & Coins**
- **Both clients generate obstacles & coins identically** based on the same logic.
- No need to **synchronize object positions over the network**.
- This **removes unnecessary network traffic** while ensuring identical gameplay for both players.

---

## **Multiplayer Implementation**
The game uses **Unity Netcode + Unity Relay** for networking:
1. **`NetworkConnect.cs`** handles **lobby creation & joining**.
2. The game **only starts when both players are ready**.
3. The **remote player mirrors local player movement** using **RPC commands**.
4. **Obstacles & coins are synced** automatically using procedural generation.
5. **Each player has their own game-over state** (one can lose while the other continues).

### **📌 RPC-Based Movement System**
Instead of sending `Vector3` positions every frame, we only send:
```csharp
[ServerRpc(RequireOwnership = false)]
public void MoveRemotePlayerServerRpc(int newLane, bool jump)
{
    MoveRemotePlayerClientRpc(newLane, jump);
}

[ClientRpc]
public void MoveRemotePlayerClientRpc(int newLane, bool jump)
{
    lane = newLane;
    if (jump && controller.isGrounded)
    {
        moveDirection.y = jumpForce;
    }
}
```
- Data Sent Per Action: sizeof(int) + sizeof(bool) = 5 bytes.
- No continuous position updates = lower bandwidth usage.
- Ensures minimal delay, even with high latency.

## **Game Flow & Interaction**
📌 Movement
- Swipe Left/Right → Changes lane (-1.5, 0, 1.5 for local, -15.5, -14, -12.5 for remote).
- Swipe Up → Jumps if the player is on the ground.

📌 Game Start Logic
- Players connect via Unity Relay.
- The game only starts when both players are ready.
- A loading screen waits for the remote player.

📌 Game Over Logic
- If one player dies, their floor stops spawning.
- The other player can continue playing, if needed, but in this case we are replicating everything locally done to remote.
- The game-over screen appears individually for each player.

## **Debugging & Network Efficiency**

📌 The game logs RPC calls and network usage:

- [Local] Sent RPC: Move to Lane 2, Position: (-1.5, 0, 10), Data Sent: 4 bytes
- [Remote] Player moved to Lane 2, Position: (-15.5, 0, 10), Data Sent: 4 bytes
- [Local] Sent RPC: Jump, Data Sent: 1 byte
- [Remote] Player Jumped, Data Sent: 1 byte

- Tracks real-time network performance.
- Helps compare optimized vs unoptimized networking.

## **Installation & Running**
📌 Prerequisites
- Unity 6  (Recommended)
- Unity Netcode & Relay package installed.

📌 Steps to Run
- Clone the Repository
- Open in Unity and install dependencies.
- Build the Game for Android/iOS or run in Play Mode.
