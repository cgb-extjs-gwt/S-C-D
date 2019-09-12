import { buildMvcUrl, post, downloadFile } from "../../Common/Services/Ajax";
import { PortfolioPivotRequest } from "../States/PortfolioPivotRequest";

export const PORTFOLIO_PIVOT_GRID_CONTROLLER_NAME = 'PortfolioPivotGrid';

export const buildGetDataUrl = () => buildMvcUrl(PORTFOLIO_PIVOT_GRID_CONTROLLER_NAME, 'GetData');

export const pivotExcelExport = (request: PortfolioPivotRequest) => {
    downloadFile(PORTFOLIO_PIVOT_GRID_CONTROLLER_NAME,'PivotExcelExport', request);
}