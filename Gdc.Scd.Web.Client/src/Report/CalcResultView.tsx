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
        return <Container layout="vbox">

            <TabPanel flex="1" tabBar={{ layout: { pack: 'left' } }}>

                <Container title="Hardware<br>service costs" layout="fit">
                    <HwCostView approved={false} />
                </Container>

                <Container title="Hardware<br>service costs<br>(approved)" layout="fit">
                    <HwCostView approved={true} />
                </Container>

                {
                    isVisibleHddNotApproved &&
                    <Container title="Hdd retention<br>service costs" layout="fit">
                        <HddCostView approved={false} />
                    </Container>
                }

                <Container title="Hdd retention<br>service costs<br>(approved)" layout="fit">
                    <HddCostView approved={true} />
                </Container>

                {
                    isVisibleSwNotApproved &&
                    <Container title="Software &amp; Solution<br>service costs" layout="fit" >
                        <SwCostView approved={false} />
                    </Container>
                }

                <Container title="Software &amp; Solution<br>service costs<br>(approved)" layout="fit">
                    <SwCostView approved={true} />
                </Container>

                <Container title="Software &amp; Solution<br>proactive cost" layout="fit">
                    <SwProactiveCostView approved={false} />
                </Container>

                <Container title="Software &amp; Solution<br>proactive cost<br>(approved)" layout="fit">
                    <SwProactiveCostView approved={true} />
                </Container>

            </TabPanel>

        </Container>;
    }
}