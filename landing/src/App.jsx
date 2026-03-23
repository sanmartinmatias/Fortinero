import { useState } from 'react'
import { Unity, useUnityContext } from "react-unity-webgl";

import './index.css'

function App() {

  const { unityProvider } = useUnityContext({
    loaderUrl: "build/myunityapp.loader.js",
    dataUrl: "build/myunityapp.data",
    frameworkUrl: "build/myunityapp.framework.js",
    codeUrl: "build/myunityapp.wasm",
  });


  return (
    <div className="min-h-screen flex flex-col items-center justify-center p-8">
      <h1 className="text-4xl font-black text-indigo-500 mb-4 tracking-tighter">
        FORTINERO
      </h1>
      <Unity unityProvider={unityProvider} />
     
      <h1 style={{ color: 'red' }} className="text-indigo-500 text-5xl">
      FORTINERO
</h1>
    </div>
    
  )
}
export default App