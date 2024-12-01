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
  <frame layout={:MainBodyLayout} border={:Theme_WindowBorder} border-thickness={:Theme_WindowBorderThickness} margin="8">
    <lane layout="stretch content" orientation="horizontal">
      <!-- for sale -->
      <scrollable layout={:ForSaleLayout} peeking="128"
                  scrollbar-margin="0,0,0,-8">
        <grid layout="stretch content" item-layout="length: 192"
              horizontal-item-alignment="middle">
          <frame *repeat={:LivestockData}
            padding="16"
            background={:^Theme_ItemRowBackground} background-tint={BackgroundTint}
            pointer-enter=|GridCell_PointerEnter()|
            pointer-leave=|GridCell_PointerLeave()|
          >
            <panel layout="160px 144px" horizontal-content-alignment="middle">
              <image layout="content 64px" margin="0,8" sprite={:ShopIcon} />
              <lane layout="stretch 64px" margin="8,88" orientation="horizontal">
                <image layout="48px 48px" sprite={:TradeItem} />
                <label layout="content 48px" text={:TradePrice} font={:TradeDisplayFont}/>
              </lane>
            </panel>
          </frame>
        </grid>
      </scrollable>
      <!-- info bot -->
      <lane *if={HasLivestock} layout="192px stretch" margin="64,0,0,0" padding="0,32" orientation="vertical" horizontal-content-alignment="middle">
        <!-- <frame layout="content[128..] content[192..]" background={:AnimBg}> -->
        <panel layout="stretch stretch" *context={HoveredLivestock}>
          <image z-index="1" layout={:AnimLayout} sprite={AnimSprite}/>
        </panel>
        <!-- </frame> -->
      </lane>
    </lane>
  </frame>

  <!-- spacer to counterbalance the portrait -->
  <spacer *if={:IsWidescreen} layout="192px 0px"/>

</lane>
