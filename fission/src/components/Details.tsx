import React from "react";
import * as THREE_ADDONS from 'three-addons';
import './Details.css'

export default function DetailsPanel({ballCount, fps, toggleSpawning}) {
    return (
        <div id="info">
            <p>Ball Count: {ballCount}</p>
            <p>{fps.toFixed(1)} FPS</p>
            <button onClick={toggleSpawning}>Toggle Spawning</button>
        </div>
    )
}