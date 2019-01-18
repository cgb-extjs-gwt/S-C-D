import { CostElementIdentifier } from "../States/CostElementIdentifier";
import { get, post } from "./Ajax";
import { NamedId } from "../States/CommonStates";
import { ImportResult } from "../../CostImport/States/ImportResult";

const COST_BLOCK_CONTROLLER = 'CostBlock';

export const getDependencyItems = (costElementId: CostElementIdentifier) => get<NamedId<number>[]>(
    COST_BLOCK_CONTROLLER,
    'GetDependencyItems',
    costElementId
)

export const importExcel = (costElementId: CostElementIdentifier, file, dependencyItemId?: number) => new Promise<ImportResult>(
    (resolve, reject) => {
        const fileReader = new FileReader();

        fileReader.onload = (event: any) => {
            const excelFileRaw: string = event.target.result;
            const data = {
                costElementId,
                dependencyItemId,
                excelFile: excelFileRaw.split('base64,')[1]
            };

            post<any, ImportResult>(COST_BLOCK_CONTROLLER, 'ImportExcel', data).then(resolve, reject);
        }

        fileReader.onerror = () => reject('file reading error');

        fileReader.readAsDataURL(file);
    }
)