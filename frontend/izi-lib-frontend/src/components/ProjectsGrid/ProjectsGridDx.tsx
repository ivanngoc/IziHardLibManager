import { DataGrid } from 'devextreme-react'
import React, { useEffect, useState } from 'react'
import { CsprojDto } from '../../types/Dtos/CsprojDto'
import { apiService } from '../../api/ApiForProjects'
import { Column, FilterPanel, FilterRow, Search, SearchPanel } from 'devextreme-react/cjs/data-grid'
import 'devextreme/dist/css/dx.light.css';  // Import DevExtreme CSS
// import 'devextreme/dist/css/dx.dark.css';  // Import DevExtreme CSS

const ProjectsGridDx = () => {
    const [datas, setDatas] = useState<CsprojDto[]>([]);

    useEffect(() => {
        apiService.getTableForProjects().then(x => {
            if (x.isSome()) {
                setDatas(x.unwrap());
            }
        });

    }, []);

    return (
        <div>
            <DataGrid
                dataSource={datas}
                allowColumnResizing={true}>
                <Column dataField='Guid'>

                </Column>
                <Column dataField='Description'></Column>
                <Column dataField='Name'></Column>
                <Column dataField='Paths'></Column>
                <Column dataField='Devices'></Column>
                <Column dataField='Tags'></Column>
                <SearchPanel visible={true} />
                {
                    // один большой фильтр
                    <FilterPanel visible={true}></FilterPanel>
                }
                {
                    // фильтры в шапке
                    <FilterRow visible={true}></FilterRow>
                }
            </DataGrid>
        </div>
    )
}

export default ProjectsGridDx