import { DataGrid } from 'devextreme-react'
import React, { useEffect, useState } from 'react'
import { apiService } from '../../api/ApiForProjects'
import { Column, FilterPanel, FilterRow, Pager, Search, SearchPanel } from 'devextreme-react/cjs/data-grid'
import 'devextreme/dist/css/dx.light.css';  // Import DevExtreme CSS
import { AsmdefDto } from '../../types/Dtos/AsmdefDto'
import { allowedPageSizes } from '../../shared/TableConfigs';
// import 'devextreme/dist/css/dx.dark.css';  // Import DevExtreme CSS


const AsmdefsGridDx = () => {
    const [datas, setDatas] = useState<AsmdefDto[]>([]);

    useEffect(() => {
        apiService.getTableForAsmdefs().then(x => {
            if (x.isSome()) {
                setDatas(x.unwrap());
            }
        });

    }, []);

    return (
        <div>
            <h1>Asmdefs</h1>
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
                <Pager allowedPageSizes={allowedPageSizes} showNavigationButtons={true} showPageSizeSelector={true}></Pager>
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

export default AsmdefsGridDx