import { DeviceDto } from "./DeviceDto";
import { TagDto } from "./TagDto";

export interface CsprojDto {
    Guid: string,
    Name?: string,
    Description?: string,
    Devices?: DeviceDto[],
    Tags?: TagDto[],
}


