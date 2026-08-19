# Unity CLI Agent Guide

This guide provides the instructions, rules, and best practices for an AI agent to interact with the Unity Editor via the Unity CLI.

## 🎯 Objective
The goal is to transform the Unity Editor from a GUI-based tool into a programmable environment, allowing the agent to **Observe**, **Act**, and **Verify** changes in real-time without manual user intervention.

---

## 🛠 Setup & Prerequisites
Before attempting to control Unity, ensure the following are in place:
1.  **Unity Version:** Unity 6.0+ is required for full pipeline support.
2.  **Unity CLI:** Installed and authenticated (`unity auth login`).
3.  **Pipeline Package:** The `com.unity.pipeline` package must be installed in the project:
    *   Command: `unity pipeline install`
4.  **Editor State:** The Unity Editor **must be open** and the project loaded for `command` and `eval` to work.
5.  **MCP Server:** For seamless tool integration, run `unity mcp`.

---

## 📖 Core Command Reference

### 1. Environment Management
Used for setting up the project and build pipeline.
*   `unity install <version>`: Install a specific editor version.
*   `unity build`: Trigger a project build.

### 2. Editor Control (`unity command`)
Used to drive the Editor's high-level functions.
*   `unity command <action>`: Trigger specific editor actions (e.g., entering play mode, saving scenes).

### 3. The REPL / C# Execution (`unity command eval`)
The most powerful tool for agents. Allows executing arbitrary C# code in the Editor context.
*   **Usage:** `unity command eval "<C# Code>"`
*   **Purpose:** Querying the scene, modifying components on the fly, and verifying state.
*   **Example (Query):** `unity command eval "return GameObject.Find(\"Player\").transform.position;"`
*   **Example (Modify):** `unity command eval "GameObject.Find(\"Floor\").GetComponent<Collider>().enabled = true;"`

---

## 🔄 The Agentic Workflow: Observe $\rightarrow$ Act $\rightarrow$ Verify

Agents should never assume a command worked. Always follow this loop:

1.  **Observe (The "Eyes"):**
    *   Use `unity command eval` to inspect the current state.
    *   *Example:* "Check if the Player has a Rigidbody." $\rightarrow$ `unity command eval "return GameObject.Find(\"Player\").GetComponent<Rigidbody>() != null;"`
2.  **Act (The "Hands"):**
    *   Use `unity command` or `unity command eval` to make the change.
    *   *Example:* "Add a Rigidbody to the Player." $\rightarrow$ `unity command eval "GameObject.Find(\"Player\").AddComponent<Rigidbody>();"`
3.  **Verify (The "Check"):**
    *   Immediately use `eval` again to confirm the change is reflected in the engine.
    *   *Example:* "Verify Rigidbody exists." $\rightarrow$ `unity command eval "return GameObject.Find(\"Player\").GetComponent<Rigidbody>() != null;"`

---

## ⚠️ Rules & Constraints

### 🛑 Safety First
*   **No Destructive Eval:** Avoid running `eval` commands that delete large numbers of assets or reset the entire scene unless explicitly requested.
*   **State Awareness:** Always check if the Editor is in **Play Mode** vs **Edit Mode** before running logic that depends on it.
*   **Null Checks:** Always write `eval` snippets with null checks to prevent the CLI from returning confusing errors.
    *   *Bad:* `GameObject.Find(\"Player\").name`
    *   *Good:* `var p = GameObject.Find(\"Player\"); return p != null ? p.name : \"Not Found\";`

### ⚙️ Operational Rules
*   **Avoid Heavy Loops in Eval:** Do not run long-running loops inside an `eval` command; it can freeze the Unity Editor.
*   **Prefer Component-Based Logic:** When modifying objects, target components specifically rather than generic GameObjects.

---

## 💡 Tips & Tricks

### 🚀 Efficiency Hacks
*   **Combine Commands:** If you need to check and set, do it in one `eval` string to reduce CLI overhead.
    *   `unity command eval "var f = GameObject.Find(\"Floor\"); if(f != null) f.GetComponent<Collider>().enabled = true; return f != null;"`
*   **Use `FindObjectsByType`:** When searching for many items, use `UnityEngine.Object.FindObjectsByType<T>()` for better performance than `GameObject.Find`.

### 🔍 Debugging the Agent
*   **Log to Console:** Use `Debug.Log` inside `eval` to send messages to the Unity Editor console for the user to see.
    *   `unity command eval "Debug.Log(\"Agent is inspecting the scene...\");"`
*   **Verify via Play Mode:** If a bug is physics-related, use `unity command` to enter Play Mode, wait a few seconds, and then use `eval` to check if positions have changed.

---

## 🔌 MCP Integration
When connected via `unity mcp`, the agent should prioritize the provided tools over raw shell commands.
*   **Tool Priority:** `mcp_tool` $\rightarrow$ `unity command eval` $\rightarrow$ `unity command`.
*   **Contextual Mapping:** Map user requests like *"Fix the lighting"* to a sequence of `eval` calls to check Light components and adjust their intensity.
