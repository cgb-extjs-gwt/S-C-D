import { CostMetaData } from "../../Common/States/CostMetaStates";
import { get } from "../../Common/Services/Ajax";
import { AppData } from "../States/AppStates";

const CONTROLLER_NAME = 'App';

export const getAppData = () => get<AppData>(CONTROLLER_NAME, 'GetAppData');