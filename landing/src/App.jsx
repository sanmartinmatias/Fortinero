import React from "react";
import { Unity, useUnityContext } from "react-unity-webgl";

function App() {
  const { unityProvider, isLoaded, loadingProgression } = useUnityContext({
    // These paths are relative to the 'public' folder
    loaderUrl: "UnityBuild/Build/Build.loader.js",
    dataUrl: "UnityBuild/Build/Build.data.gz",
    frameworkUrl: "UnityBuild/Build/Build.framework.js.gz",
    codeUrl: "UnityBuild/Build/Build.wasm.gz",
  });

  // Calculate loading percentage
  const loadingPercentage = Math.round(loadingProgression * 100);

  return (
    <div className="min-h-screen flex flex-col items-center justify-center p-8 bg-slate-900">
      <h1 className="text-4xl font-black text-indigo-500 mb-4 tracking-tighter">
        FORTINERO
      </h1>

      <div className="relative w-[960px] h-[600px] bg-black rounded-lg overflow-hidden shadow-2xl">
        {/* Loading Overlay */}
        {!isLoaded && (
          <div className="absolute inset-0 flex flex-col items-center justify-center bg-slate-800 text-white z-10">
            <p className="mb-2">Loading Fortinero... {loadingPercentage}%</p>
            <div className="w-64 h-2 bg-gray-700 rounded-full">
              <div 
                className="h-full bg-indigo-500 transition-all duration-300" 
                style={{ width: `${loadingPercentage}%` }}
              />
            </div>
          </div>
        )}

        <Unity 
          unityProvider={unityProvider} 
          style={{ width: "100%", height: "100%" }}
        />
      </div>
      
      <h1 className="text-indigo-400 text-xl mt-6 opacity-50">
        WEBGL EDITION
      </h1>
    </div>
  );
}

export default App;