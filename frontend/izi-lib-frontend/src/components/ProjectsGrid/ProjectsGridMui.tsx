import {
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    Paper,
    TextField,
    IconButton,
} from "@mui/material";
import { GridColDef, GridRowsProp } from '@mui/x-data-grid'
import { DataGrid } from '@mui/x-data-grid/DataGrid'
import React, { useEffect, useState } from 'react'
import { apiService } from '../../api/ApiForProjects'
import { CsprojDto } from '../../types/Dtos/CsprojDto'

type Props = {
    some?: any,
}

const ProjectsGrid = (props: Props) => {
    const [datas, setDatas] = useState<CsprojDto[]>([]);
    const [rows, setRows] = useState<GridRowsProp>([]);

    // const rowsExample: GridRowsProp = [
    //     { id: 1, col1: 'Hello', col2: 'World' },
    //     { id: 2, col1: 'DataGridPro', col2: 'is Awesome' },
    //     { id: 3, col1: 'MUI', col2: 'is Amazing' },
    // ];

    // const rows: GridRowsProp = [
    //     { id: 1, col1: 'Guid', col2: 'World' },
    //     { id: 2, col1: 'DataGridPro', col2: 'is Awesome' },
    //     { id: 3, col1: 'MUI', col2: 'is Amazing' },
    // ];

    const columns: GridColDef[] = [
        { field: 'Guid', headerName: 'Guid', width: 150 },
        { field: 'Name', headerName: 'Name', width: 150 },
        { field: 'Description', headerName: 'Description', width: 150 },
        { field: 'Devices', headerName: 'Devices', width: 150 },
        { field: 'Tags', headerName: 'Tags', width: 150 },
    ];

    useEffect(() => {
        apiService.getTableForProjects().then(x => {
            if (x.isSome()) {
                setDatas(x.unwrap());
            }
        });

    }, []);

    useEffect(() => {
        if (datas.length > 0) {
            const rows: GridRowsProp = datas.map((x, index) => {
                return { id: index, Guid: x.Guid, Description: x.Description, Devices: x.Devices }
            });
            setRows(rows);
        }
    }, [datas]);

    return (
        <div>
            {
                //https://mui.com/material-ui/react-table/
            }
            {/* <TextField
                label="Search"
                variant="outlined"
                fullWidth
                margin="normal"
                value={searchQuery}
                onChange={handleSearch}
            /> */}
            <TableContainer component={Paper}>
                <Table>
                    <DataGrid rows={rows} columns={columns}></DataGrid>
                </Table >
            </TableContainer>
        </div>
    )
}

export default ProjectsGrid