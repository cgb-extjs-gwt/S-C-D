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

const TAB_LINK = {
    Hw_cost: 'Hw_cost',
    Hw_cost_approved: 'Hw_cost_approved',
    Hdd_cost: 'Hdd_cost',
    Hdd_cost_approved: 'Hdd_cost_approved',
    Sw_cost: 'Sw_cost',
    Sw_costs_approved: 'Sw_costs_approved',
    Sw_proactive_cost: 'Sw_proactive_cost',
    Sw_proactive_cost_approved: 'Sw_proactive_cost_approved'
};

export class CalcResultView extends React.Component<CalcResultViewProps> {

    public constructor(props: any) {
        super(props);
        this.init();
    }

    public state = {
        activeTab: 'Hw_cost'
    }

    public render() {
        const { isVisibleHddNotApproved, isVisibleSwNotApproved } = this.props;

        //render 1 component only, not all components in tabs

        return <Container layout="vbox">

            <TabPanel fullscreen={false} tabBar={{ layout: { pack: 'left' } }} onActiveItemChange={this.onTabChange} >

                <Container title="Hardware<br>service costs" name={TAB_LINK.Hw_cost} />
                <Container title="Hardware<br>service costs<br>(approved)" name={TAB_LINK.Hw_cost_approved} />
                {
                    isVisibleHddNotApproved &&
                    <Container title="Hdd retention<br>service costs" name={TAB_LINK.Hdd_cost} />
                }
                <Container title="Hdd retention<br>service costs<br>(approved)" name={TAB_LINK.Hdd_cost_approved} />
                {
                    isVisibleSwNotApproved &&
                    <Container title="Software &amp; Solution<br>service costs" name={TAB_LINK.Sw_cost} />
                }
                <Container title="Software &amp; Solution<br>service costs<br>(approved)" name={TAB_LINK.Sw_costs_approved} />
                <Container title="Software &amp; Solution<br>proactive cost" name={TAB_LINK.Sw_proactive_cost} />
                <Container title="Software &amp; Solution<br>proactive cost<br>(approved)" name={TAB_LINK.Sw_proactive_cost_approved} />

            </TabPanel>

            <Container flex="2" layout="fit">
                {this.getTabCmp()}
            </Container>

        </Container>;
    }

    private init() {
        this.onTabChange = this.onTabChange.bind(this);
    }

    private getTabCmp() {
        let tab = this.state.activeTab;
        switch (tab) {
            case TAB_LINK.Hw_cost: return <HwCostView approved={false} />;
            case TAB_LINK.Hw_cost_approved: return <HwCostView approved={true} />;
            case TAB_LINK.Hdd_cost: return <HddCostView approved={false} />;
            case TAB_LINK.Hdd_cost_approved: return <HddCostView approved={true} />;
            case TAB_LINK.Sw_cost: return <SwCostView approved={false} />;
            case TAB_LINK.Sw_costs_approved: return <SwCostView approved={true} />;
            case TAB_LINK.Sw_proactive_cost: return <SwProactiveCostView approved={false} />;
            case TAB_LINK.Sw_proactive_cost_approved: return <SwProactiveCostView approved={true} />;
            default: return null;
        }
    }

    private onTabChange(sender, tab) {
        this.setState({ activeTab: tab.name });
    }
}