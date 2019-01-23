import { CostElementIdentifier } from "../States/CostElementIdentifier";
import { get, post } from "./Ajax";
import { NamedId } from "../States/CommonStates";
import { ImportResult } from "../../CostImport/States/ImportResult";
import { CostElementData } from "../States/CostElementData";

const COST_BLOCK_CONTROLLER = 'CostBlock';

export const getCostElementData = (costElementId: CostElementIdentifier) => get<CostElementData>(
    COST_BLOCK_CONTROLLER,
    'GetCostElementData',
    costElementId
)

export const importExcel = (costElementId: CostElementIdentifier, file, dependencyItemId?: number, regionId?: number) => new Promise<ImportResult>(
    (resolve, reject) => {
        const fileReader = new FileReader();

        fileReader.onload = (event: any) => {
            const excelFileRaw: string = event.target.result;
            const data = {
                costElementId,
                dependencyItemId,
                regionId,
                excelFile: excelFileRaw.split('base64,')[1]
            };

            post<any, ImportResult>(COST_BLOCK_CONTROLLER, 'ImportExcel', data).then(resolve, reject);
        }

        fileReader.onerror = () => reject('file reading error');

        fileReader.readAsDataURL(file);
    }
)