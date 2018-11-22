import { connect } from "react-redux";
import { QualityGateToolbarActions } from "../../QualityGate/Components/QualityGateToolbar";
import { CommonState } from "../../Layout/States/AppStates";
import { QualtityGateSetProps, QualtityGateTab } from "./QualtityGateSetView";
import { getCostBlock, getCostElement } from "../../Common/Helpers/MetaHelper";
import { saveTableViewToServer } from "../Actions/TableViewActions";

export const QualtityGateSetContainer =
    connect<QualtityGateSetProps, QualityGateToolbarActions, QualityGateToolbarActions, CommonState>(
        ({ app: { appMetaData }, pages: { tableView } }) => {
            const { qualityGateResultSet, info: { recordInfo } } = tableView;

            const tabs: QualtityGateTab[] = [];

            if (qualityGateResultSet && qualityGateResultSet.hasErros) {
                for (const item of qualityGateResultSet.items) {
                    if (item.qualityGateResult.hasErrors) {
                        const { applicationId, costBlockId, costElementId } = item.costElementIdentifier;
                        
                        const fieldInfos = recordInfo.data.filter(
                            fieldInfo => 
                                fieldInfo.applicationId == applicationId &&
                                fieldInfo.metaId == costBlockId &&
                                fieldInfo.fieldName == costElementId
                        );

                        const costBlock = getCostBlock(appMetaData, costBlockId);
                        const costElement = getCostElement(costBlock, costElementId);

                        tabs.push(...fieldInfos.map(fieldInfo => <QualtityGateTab>{
                            key: `${applicationId}_${costBlockId}_${costElementId}`,
                            title: `${costBlock.name} ${costElement.name}`,
                            costElement,
                            errors: item.qualityGateResult.errors
                        }));
                    }
                }
            }

            return <QualtityGateSetProps>{
                tabs
            }
        },
        (dispatch, { onCancel, onSave }) => ({
            onCancel: onCancel,
            onSave: explanationMessage => saveTableViewToServer({ 
                hasQualityGateErrors: true, 
                isApproving: true,
                qualityGateErrorExplanation: explanationMessage
            })
        })
    )