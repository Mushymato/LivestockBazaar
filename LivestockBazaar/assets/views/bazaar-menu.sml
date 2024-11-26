<lane orientation="horizontal">
  <!-- Shop Owner Portrait -->
  <lane *if={:ShouldDisplayOwner} layout="320px content" padding="0,36,0,0" orientation="vertical" horizontal-content-alignment="end">
    <frame padding="20" background={:Theme_PortraitBackground}>
      <image layout="256px 256px" sprite={:OwnerPortrait} />
    </frame>
    <frame layout="320px content[..320]" padding="20" margin="0,10" border={:Theme_DialogueBackground}>
      <label font="dialogue" text={:OwnerDialog} color={:Theme_DialogueColor} shadow-color={:Theme_DialogueShadowColor}/>
    </frame>
  </lane>

  <!-- Main Panel -->
  <frame layout={:ForSaleLayout} border={:Theme_WindowBorder} border-thickness={:Theme_WindowBorderThickness} margin="8">
    <scrollable layout="stretch stretch" peeking="128">
      <grid layout="stretch content" item-layout="length: 192"
            horizontal-item-alignment="middle">
        <frame *repeat={:LivestockData}
          padding="16"
          background={:^Theme_ItemRowBackground} background-tint={BackgroundTint}
          pointer-enter=|PointerEnter()|
          pointer-leave=|PointerLeave()|
        >
          <panel layout="160px 144px" tooltip={:ShopDisplayName} horizontal-content-alignment="middle">
            <image layout="content 64px" sprite={:ShopIcon} />
            <lane layout="content 80px" margin="0,64" padding="0,16" orientation="horizontal">
              <image layout="48px 48px" sprite={:TradeItem} />
              <label layout="content 48px" text={:TradePrice} font={:TradeDisplayFont}/>
            </lane>
          </panel>
        </frame>
      </grid>
    </scrollable>
  </frame>

  <!-- spacer for centering -->
  <spacer *if={:ShouldDisplayOwner} layout="296px 0px"/>

</lane>
