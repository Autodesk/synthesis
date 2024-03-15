import { useEffect, useRef } from "react";
import GetSceneRenderer from "../systems/scene/SceneRenderer";
import Stats from 'stats.js';

let stats: Stats | null;

class SceneProps {
    public useStats = false;
}

function Scene({ useStats }: SceneProps) {
    const refContainer = useRef<HTMLDivElement>(null);

    useEffect(() => {
        if (refContainer.current) {
            const sr = GetSceneRenderer();

            refContainer.current.innerHTML = "";
            refContainer.current.appendChild(sr.renderer.domElement)

            if (useStats) {
                stats = new Stats();
                stats.dom.style.position = 'absolute';
                stats.dom.style.top = '0px';
                refContainer.current.appendChild(stats.dom);
            }
        }
    }, [useStats]);

    return (
        <div>
            <div ref={refContainer}></div>
        </div>
    );
}

export default Scene;