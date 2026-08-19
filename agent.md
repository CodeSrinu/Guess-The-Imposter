# 🚀 Unity CLI Agent Guide: Let's Build This!

Welcome to the team, bro! This guide is here to make sure we're on the same page when we're controlling the Unity Editor from the CLI. We're turning Unity from a "click-and-drag" tool into a programmable beast.

## 🎯 The Game Plan
Our goal is simple: **Observe $\rightarrow$ Act $\rightarrow$ Verify**. 
We don't guess. We don't hope. We check the state, make the change, and then double-check that it actually worked.

---

## 🛠 Getting Your Gear Ready
Before you start messing with Unity, make sure this stuff is sorted:
1.  **Unity Version:** Unity 6.0+ (we need this for the pipeline magic).
2.  **Unity CLI:** Installed and logged in (`unity auth login`).
3.  **The Pipeline Package:** Install `com.unity.pipeline` so the CLI can actually talk to the Editor:
    *   Just run: `unity pipeline install`
4.  **Editor State:** Unity **must be open** and the project loaded. No editor, no control.
5.  **MCP Server:** Run `unity mcp` to let the AI agent use these tools natively.

---

## 📖 The Toolkit (Command Reference)

### 1. The Basics (Environment)
Setting up the project or hitting the build button.
*   `unity install <version>`: Grab a specific editor version.
*   `unity build`: Trigger the build process.

### 2. Driving the Editor (`unity command`)
For those high-level actions.
*   `unity command <action>`: Things like entering play mode or saving the scene.

### 3. The Magic Wand (`unity command eval`)
This is your most powerful tool. It lets you run arbitrary C# code directly in the Editor.
*   **How to use it:** `unity command eval "<C# Code>"`
*   **What it's for:** Checking scene data, flipping components on/off, and verifying your fixes.
*   **Example (Checking):** `unity command eval "return GameObject.Find(\"Player\").transform.position;"`
*   **Example (Fixing):** `unity command eval "GameObject.Find(\"Floor\").GetComponent<Collider>().enabled = true;"`

---

## 🔄 The Workflow: Observe $\rightarrow$ Act $\rightarrow$ Verify

Never assume a command worked. Always follow this loop, bro:

1.  **Observe (The "Eyes"):**
    *   Use `eval` to see what's actually happening.
    *   *Example:* "Does the Player even have a Rigidbody?" $\rightarrow$ `unity command eval "return GameObject.Find(\"Player\").GetComponent<Rigidbody>() != null;"`
2.  **Act (The "Hands"):**
    *   Use `command` or `eval` to make the change.
    *   *Example:* "Let's add that Rigidbody." $\rightarrow$ `unity command eval "GameObject.Find(\"Player\").AddComponent<Rigidbody>();"`
3.  **Verify (The "Check"):**
    *   Immediately use `eval` again. Did it work?
    *   *Example:* "Check it one more time." $\rightarrow$ `unity command eval "return GameObject.Find(\"Player\").GetComponent<Rigidbody>() != null;"`

---

## ⚠️ The "Don't Do This" List (Rules)

### 🛑 Safety First
*   **Don't be Destructive:** Avoid `eval` commands that wipe the scene or delete tons of assets unless you're 100% sure.
*   **Know Your Mode:** Always check if you're in **Play Mode** or **Edit Mode** before running logic.
*   **Null Checks are Life:** Always write `eval` snippets that handle nulls, otherwise, the CLI will just scream at you.
    *   *Bad:* `GameObject.Find(\"Player\").name`
    *   *Good:* `var p = GameObject.Find(\"Player\"); return p != null ? p.name : \"Not Found\";`

### ⚙️ Pro Tips for Stability
*   **No Heavy Loops:** Don't run massive loops in `eval`; you'll freeze the whole Unity Editor.
*   **Target Components:** Instead of generic GameObjects, target the specific components you need.

---

## 💡 Pro Tips & Tricks

### 🚀 Speed Hacks
*   **Combine your logic:** If you can check and set in one go, do it! It saves time.
    *   `unity command eval "var f = GameObject.Find(\"Floor\"); if(f != null) f.GetComponent<Collider>().enabled = true; return f != null;"`
*   **FindObjectsByType:** Use `UnityEngine.Object.FindObjectsByType<T>()` instead of `GameObject.Find` when you're hunting for multiple things.

### 🔍 Debugging like a Pro
*   **Talk to the Console:** Use `Debug.Log` in your `eval` calls to leave notes for the user in the Unity Console.
    *   `unity command eval "Debug.Log(\"Agent is checking the scene, hang tight...\");"`
*   **Physics Check:** If something feels wrong with physics, enter Play Mode via `unity command`, wait a bit, and then use `eval` to check the positions.

---

## 🔌 MCP Integration
When `unity mcp` is running, use the provided tools first—they're way smoother than raw shell commands.
*   **Priority:** `mcp_tool` $\rightarrow$ `unity command eval` $\rightarrow$ `unity command`.
*   **The Mindset:** If the user says *"The lighting is off,"* your brain should go: `eval` (check light intensity) $\rightarrow$ `eval` (adjust intensity) $\rightarrow$ `eval` (verify).
