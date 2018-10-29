import { connect } from "react-redux";
import { QualityGateErrorProps, QualityGateErrorActions, QualityGateErrorView } from "./QualityGateErrorView";
import { CommonState } from "../../Layout/States/AppStates";
import { getDependecyColumnFromMeta } from "../../Common/Helpers/ColumnInfoHelper";
import { ColumnInfo, ColumnType } from "../../Common/States/ColumnInfo";

export interface QualityGateErrorContainerProps extends QualityGateErrorActions {
    costBlockId: string
    costElementId: string
    errors?: {[key: string]: any}[]
}

export const QualityGateErrorContainer = 
    connect<QualityGateErrorProps, QualityGateErrorActions, QualityGateErrorContainerProps, CommonState>(
        (state, { costBlockId, costElementId, errors }) => {
            const meta = state.app.appMetaData;
            let columns: ColumnInfo[] = [];

            if (meta) {
                columns = [
                    { title: 'Wg', dataIndex: `WarrantyGroupName`, type: ColumnType.Text },
                    getDependecyColumnFromMeta(meta, costBlockId, costElementId),
                    { title: 'Period error', dataIndex: `IsPeriodError`, type: ColumnType.CheckBox },
                    { title: 'Country group error', dataIndex: `IsRegionError`, type: ColumnType.CheckBox },
                ];
            }

            return <QualityGateErrorProps> {
                columns,
                errors
            };
        },
        (dispatch, { costBlockId, onSave, onCancel }) => ({
            onSave: explanationMessage => onSave && onSave(explanationMessage),
            onCancel: () => onCancel && onCancel()
        })
    )(QualityGateErrorView)