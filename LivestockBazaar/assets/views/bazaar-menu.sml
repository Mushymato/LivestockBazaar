<lane orientation="horizontal">
  <!-- shop owner portrait -->
  <lane *if={:IsWidescreen} layout="320px content" padding="0,8,0,0" orientation="vertical" horizontal-content-alignment="end">
    <frame *if={:ShowPortraitBox} padding="20" background={:Theme_PortraitBackground}>
      <image layout="256px 256px" sprite={:OwnerPortrait} />
    </frame>
    <frame layout="256px content[..320]" padding="20" margin="0,10" border={:Theme_DialogueBackground}>
      <label font="dialogue" text={:OwnerDialog} color={:Theme_DialogueColor} shadow-color={:Theme_DialogueShadowColor}/>
    </frame>
  </lane>

  <!-- main body -->
  <frame *switch={CurrentPage}
         layout={:MainBodyLayout} border={:Theme_WindowBorder}
         border-thickness={:Theme_WindowBorderThickness} margin="8">
    <!-- page 1 -->
    <lane *case="1" layout="stretch content" orientation="horizontal">
      <!-- for sale -->
      <scrollable layout={:ForSaleLayout} peeking="128"
                  scrollbar-margin="278,0,0,-8"
                  scrollbar-up-sprite={:Theme_ScrollUp}
                  scrollbar-down-sprite={:Theme_ScrollDown}
                  scrollbar-down-sprite={:Theme_ScrollDown}
                  scrollbar-thumb-sprite={:Theme_ScrollBarFront}
                  scrollbar-track-sprite={:Theme_ScrollBarBack}>
        <grid layout="stretch content" item-layout="length: 192"
              horizontal-item-alignment="middle">
          <frame *repeat={:LivestockData}
            padding="16"
            background={:^Theme_ItemRowBackground} background-tint={BackgroundTint}
            pointer-enter=|GridCell_PointerEnter()|
            pointer-leave=|GridCell_PointerLeave()|
            left-click=|GridCell_LeftClick()|
          >
            <panel layout="160px 144px" horizontal-content-alignment="middle" focusable="true">
              <image layout="content 64px" margin="0,8" sprite={:ShopIcon} />
              <lane layout="stretch 64px" margin="8,88" orientation="horizontal">
                <image layout="48px 48px" sprite={:TradeItem} />
                <label layout="content 48px" text={:TradePrice} font={:TradeDisplayFont}/>
              </lane>
            </panel>
          </frame>
        </grid>
      </scrollable>
      <!-- info box -->
      <infobox *context={HoveredLivestock} />
    </lane>
    <!-- page 2 -->
    <lane *case="2" layout="stretch 70%[676..]" orientation="horizontal">
      <infobox *context={SelectedLivestock}>
        <button margin="16"
                text="BACK"
                hover-background={@Mods/StardewUI/Sprites/ButtonLight}
                left-click=|^ClearSelectedLivestock()| />
      </infobox>
    </lane>
  </frame>

  <!-- spacer to counterbalance the portrait -->
  <spacer *if={:IsWidescreen} layout="192px 0px"/>
</lane>

<template name="infobox">
  <lane layout="256px stretch"  orientation="vertical" horizontal-content-alignment="middle">
    <panel layout="content content[128..]" margin="8,8,0,0" horizontal-content-alignment="middle" vertical-content-alignment="end">
      <image layout={:AnimLayout} sprite={AnimSprite}/>
    </panel>
    <label text={:DisplayName} font="dialogue"/>
    <label text={:Description} font="small" margin="8,0" />
    <outlet/>
  </lane>
</template>
