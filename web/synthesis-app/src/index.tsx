import React from 'react';
import ReactDOM from 'react-dom/client';
import './index.css';
import App from './App.tsx';
import reportWebVitals from './reportWebVitals.tsx';
// import { wasmWrapper } from './WasmWrapper.mjs';
// import { getBinaryFile } from './util/FileLoading.mjs';

function test() {

    console.log("Helloskfjdks");

    // await wasmWrapper.wrapperPromise;

    // var miraFile = getBinaryFile("TestCube_v1.mira");
    // var assembly = wasmWrapper.parseAssembly(await miraFile);
    // wasmWrapper.debugPrintAssembly(assembly);
    // wasmWrapper.destroyAssembly(assembly);

    

    // wasmWrapper.coreDestroy();
}

const root = ReactDOM.createRoot(document.getElementById('root') as HTMLElement);
root.render(
    <React.StrictMode>
        <App />
        <button onClick={() => test()}>Load Mira</button>
    </React.StrictMode>
);

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals(null);
