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


                    <Container title="Software &amp; Solution<br>service costs<br>(approved)" layout="fit">
                        <SwCostView approved={true} />
                    </Container>

                    <Container title="Software &amp; Solution<br>proactive cost" layout="fit">
                        <SwProactiveCostView approved={false} />
                    </Container>

                </TabPanel>

            </Container>
        );
    }
}