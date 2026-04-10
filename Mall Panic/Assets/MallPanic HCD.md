## **Game Title:** Mall Panic

---

## **1. High Concept**

A fast-paced 3D top-down game where the player competes against AI-controlled shoppers inside a store to collect items and reach the cashier before time runs out.

---

## **2. Genre**

- Arcade
    
- Strategy
    
- AI Simulation
    

---

## **3. Core Gameplay**

The player moves around a 3D store from a fixed top-down view to collect items while AI shoppers do the same.  
The goal is to gather better items and reach the cashier before the timer ends.

---

## **4. Game Objectives**

- Collect items around the store
    
- Avoid delays caused by NPCs
    
- Reach the cashier before time runs out
    
- Finish with a higher score
    

---

## **5. Core Mechanics**

- 3D player movement on a flat plane (X and Z axis)
    
- Fixed top-down camera (no control)
    
- Item pickup system
    
- Timer-based gameplay
    
- Scoring system:
    
    - Score = Valuable Items − Low-value Items
        
- Cashier interaction to finalize score
    

---

## **6. Game Systems (AI Focus)**

### **6.1 Vector Motion**

- Controls movement on X and Z axes
    
- Used by both player and NPCs
    
- Handles speed and direction
    

---

### **6.2 NavMesh**

- Main navigation system
    
- Defines walkable areas on the map
    
- Prevents movement through shelves and obstacles
    
- Allows smooth pathfinding to targets
    

---

### **6.3 Pathfinding Behaviors**

NPC behavior includes:

- Moving toward nearest or most valuable items
    
- Changing routes when blocked
    
- Switching targets dynamically
    

---

### **6.4 Finite State Machine (FSM)**

Each NPC operates using states:

- **Idle** – deciding next action
    
- **Move to Item** – navigating to item
    
- **Collect Item** – picking up item
    
- **Move to Cashier** – heading to exit
    
- **Blocked/Reroute** – recalculating path
    

---

### **6.5 Crowds**

- NPCs avoid collisions with each other
    
- Movement slows in crowded areas
    
- Agents adjust paths to prevent overlap
    
- Creates realistic congestion in aisles
    

---

## **7. Controls**

- WASD / Arrow Keys – Move player
    
- (Optional) Key input – Pick up items
    

---

## **8. Game Flow**

1. Game starts → Player and NPCs spawn
    
2. Items are placed around the store
    
3. Player and NPCs collect items
    
4. Timer counts down
    
5. Player must reach the cashier before time runs out
    
6. Score is calculated
    
7. End of round
    

---

## **9. Visual Style**

- 3D top-down perspective
    
- Fixed overhead camera
    
- Simple store layout using pre-made assets
    
- Basic UI (timer and score)
    

---

## **10. Scope**

- Small 3D store map
    
- One player and a few NPCs
    
- Focus on AI behavior and navigation
    
- Minimal UI and interaction complexity
    

---