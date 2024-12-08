import { DeviceDto } from "./DeviceDto";
import { TagDto } from "./TagDto";

export interface AsmdefDto {
    Guid: string;
    Name?: string;
    Description?: string;
    Devices?: DeviceDto[];
    Tags?: TagDto[];
}
