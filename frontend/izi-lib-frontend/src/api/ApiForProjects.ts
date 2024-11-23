import { myAxios } from "../networking/myaxios";
import { some, Option } from "../types/Option";


export const getProjects = async (guid: string): Promise<Option<any>> => {
    const prom = await myAxios.get(`/api/ProjectsAtDevice?guid=${guid}`);
    return some(prom);
}