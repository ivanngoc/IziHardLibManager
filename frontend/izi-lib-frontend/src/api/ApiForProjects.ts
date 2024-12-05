import { myAxios } from "../networking/myaxios";
import { CsprojDto } from "../types/Dtos/CsprojDto";
import { some, Option } from "../types/Option";


const getProjects = async (guid: string): Promise<Option<any>> => {
    const prom = await myAxios.get(`/api/ProjectsAtDevice?guid=${guid}`);
    return some(prom);
}

const getTableForProjects = async (): Promise<Option<CsprojDto[]>> => {
    const prom = await myAxios.get(`/api/Tables/Csprojs`);
    return some(prom.data);
}

export const apiService = {
    getProjects,
    getTableForProjects
};