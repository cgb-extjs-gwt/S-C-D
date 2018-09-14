import { connect } from "react-redux";
import { QualityGateErrorProps, QualityGateErrorActions, QualityGateErrorView } from "./QualityGateErrorView";
import { CommonState } from "../../Layout/States/AppStates";
import { getDependecyColumnsFromMeta } from "../../Common/Helpers/ColumnInfoHelper";
import { ColumnInfo, ColumnType } from "../../Common/States/ColumnInfo";

export interface QualityGateErrorContainerProps extends QualityGateErrorActions {
    costBlockId: string
    errors?: {[key: string]: any}[]
}

export const QualityGateErrorContainer = 
    connect<QualityGateErrorProps, QualityGateErrorActions, QualityGateErrorContainerProps, CommonState>(
        (state, { costBlockId, errors }) => {
            const meta = state.app.appMetaData;
            let columns: ColumnInfo[] = [];

            if (meta) {
                columns = [
                    { title: 'Wg', dataIndex: `WarrantyGroupName`, type: ColumnType.Simple },
                    ...getDependecyColumnsFromMeta(meta, costBlockId),
                    { title: 'Period error', dataIndex: `IsPeriodError`, type: ColumnType.Checkbox },
                    { title: 'Country group error', dataIndex: `IsRegionError`, type: ColumnType.Checkbox },
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