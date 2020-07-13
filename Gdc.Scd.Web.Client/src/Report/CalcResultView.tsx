import { Container, TabPanel } from "@extjs/ext-react";
import * as React from "react";
import { HddCostView } from "./HddCostView";
import { HwCostView } from "./HwCostView";
import { SwCostView } from "./SwCostView";
import { SwProactiveCostView } from "./SwProactiveCostView";

export interface CalcResultViewProps {
    isVisibleHddNotApproved: boolean
    isVisibleSwNotApproved: boolean
}

export class CalcResultView extends React.Component<CalcResultViewProps> {
    public render() {
        const { isVisibleHddNotApproved, isVisibleSwNotApproved } = this.props;
                    //<Container title="Hardware<br>service costs" layout="fit">
                    //    <HwCostView approved={false} />
                    //</Container>

        return (
            <Container layout="vbox">

                <TabPanel flex="1" tabBar={{ layout: { pack: 'left' } }}>


                    <Container title="Hdd retention<br>service costs<br>(approved)" layout="fit">
                        <HddCostView approved={true} />
                    </Container>

                </TabPanel>

            </Container>
        );
    }
}