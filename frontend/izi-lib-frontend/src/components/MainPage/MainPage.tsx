import React, { useState } from 'react'
import ProjectsGridDx from '../ProjectsGrid/ProjectsGridDx'
import { TabPanel } from 'devextreme-react'
import AsmdefsGridDx from '../ProjectsGrid/AsmdefsGridDx'

type Props = {

}

const MainPage = (props: Props) => {
    const [selectedIndex, setSelectedIndex] = useState(ETabs.Csprojs);


    const tabs = [
        { title: <b>🏠 Csprojs</b>, content: 'Welcome to the home page!' },
        { title: <i>👤 Asmdefs</i>, content: 'View your profile information here.' },
        { title: <u>⚙️ Metas</u>, content: 'Adjust your preferences.' },
    ];

    return (
        <>
            <h1>Selected index: {selectedIndex}</h1>
            <TabPanel
                dataSource={tabs}
                selectedIndex={selectedIndex}
                onSelectionChanged={(e) => setSelectedIndex(e.component.option('selectedIndex') as ETabs)}
                itemTitleRender={(data) => data.title} // Render custom HTML or React components
            />
            {selectedIndex == ETabs.Csprojs && <ProjectsGridDx></ProjectsGridDx>}
            {selectedIndex == ETabs.Asmdefs && <AsmdefsGridDx></AsmdefsGridDx>}
        </>
    )
}

export enum ETabs {
    Csprojs = 0,
    Asmdefs = 1,
    Metas = 2,
}

export default MainPage