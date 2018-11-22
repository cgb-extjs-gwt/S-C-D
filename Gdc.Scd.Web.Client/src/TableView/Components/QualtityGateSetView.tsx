import * as React from "react";
import { TabPanel, Container } from "@extjs/ext-react";
import { CostElementMeta } from "../../Common/States/CostMetaStates";
import { QualityGateGrid } from "../../QualityGate/Components/QualityGateGrid";
import { BundleDetailGroup } from "../../QualityGate/States/QualityGateResult";
import { WgInputLevel } from "../../Common/Constants/MetaConstants";
import { QualityGateToolbar, QualityGateToolbarActions } from "../../QualityGate/Components/QualityGateToolbar";

export interface QualtityGateTab {
    title: string
    key: string
    costElement: CostElementMeta
    errors: BundleDetailGroup[]
}

export interface QualtityGateSetProps extends QualityGateToolbarActions {
    tabs: QualtityGateTab[]
}

export class QualtityGateSetView extends React.Component<QualtityGateSetProps> {
    public render() {
        const { tabs, onSave, onCancel } = this.props;

        return (
            <Container layout="vbox">
                <TabPanel tabBar={{layout: { pack: 'left' }}} flex={10}>
                    {
                        tabs.map(({ title, key, costElement, errors }) => (
                            <Container title={title}>
                                <QualityGateGrid
                                    key={key} 
                                    costElement={costElement} 
                                    storeConfig={{ data: errors }} 
                                    inputLevelId={WgInputLevel} 
                                    flex={1}
                                />
                            </Container>
                        ))
                    }
                </TabPanel>
                
                <QualityGateToolbar onSave={onSave} onCancel={onCancel} flex={1}/>
            </Container>
        );
    }
}