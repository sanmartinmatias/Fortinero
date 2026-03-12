Fortinero Monorepo
Fortinero is a game about expanding the frontier, managing militias and forts, and venturing into the unknown.

🛠 Project Structure & Tech Stack
/game: Unity (C#)

/landing: React

/server: Node.js

🚀 Build Instructions
1. The Server (Node.js)
The backend manages the game logic and frontier state.

Navigate to the directory: cd server

Install dependencies: npm install

Start the server: npm start (or npm run dev if using nodemon)

2. The Landing Page (React)
The web interface that hosts the game client.

Navigate to the directory: cd landing

Install dependencies: npm install

Build the production bundle: npm run build

Note: For local development, use npm start.

3. The Game (Unity)
The core gameplay experience.

Open Unity Hub.

Click Add and select the /game folder.

In the Unity Editor, go to File > Build Settings.

Select WebGL as the platform.

Click Build and target the public folder in your /landing directory to host it on the site.