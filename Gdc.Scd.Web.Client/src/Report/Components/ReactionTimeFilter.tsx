import { DictFilter, DictFilterProps } from "./DictFilter";

export class ReactionTimeFilter extends DictFilter {
    public constructor(props: DictFilterProps) {
        super({
            ...props,
            getItems: x => x.getReactionTimeTypes()
        });
    }
}