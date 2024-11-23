import Table from '@mui/material/Table'
import { GridColDef, GridRowsProp } from '@mui/x-data-grid'
import { DataGrid } from '@mui/x-data-grid/DataGrid'
import React from 'react'

type Props = {
    some?: any,
}

const ProjectsGrid = (props: Props) => {
    const rows: GridRowsProp = [
        { id: 1, col1: 'Hello', col2: 'World' },
        { id: 2, col1: 'DataGridPro', col2: 'is Awesome' },
        { id: 3, col1: 'MUI', col2: 'is Amazing' },
    ];

    const columns: GridColDef[] = [
        { field: 'col1', headerName: 'Column 1', width: 150 },
        { field: 'col2', headerName: 'Column 2', width: 150 },
    ];

    return (
        <div>
            {
                //https://mui.com/material-ui/react-table/
            }
            <Table>
                <DataGrid rows={rows} columns={columns}></DataGrid>
            </Table >
        </div>
    )
}

export default ProjectsGrid