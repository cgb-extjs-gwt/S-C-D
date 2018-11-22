import { buildMvcUrl } from "../../Common/Services/Ajax";
import { Context } from "../../Common/States/Context";
import { CostElementIdentifier } from "../../Common/States/CostElementIdentifier";

const COST_BLOCK_HISTORY_CONTROLLER_NAME = 'CostBlockHistory';

export const buildGetCostEditorHistoryUrl = (context: Context, editItemId: string) => 
    buildMvcUrl(COST_BLOCK_HISTORY_CONTROLLER_NAME, 'GetCostEditorHistory', { ...context, editItemId });

export const buildGetTableViewHistoryUrl = (costElementId: CostElementIdentifier, coordinates: { [key: string]: number }) => 
    buildMvcUrl(COST_BLOCK_HISTORY_CONTROLLER_NAME, 'GetTableViewHistory', { ...costElementId, ...coordinates });