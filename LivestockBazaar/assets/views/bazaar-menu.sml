<lane orientation="horizontal" >
  <!-- shop owner portrait -->
  <lane *if={:IsWidescreen} *float="before" layout="320px content" padding="0,8,0,0" orientation="vertical" horizontal-content-alignment="end">
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
      <scrollable-styled layout={:~BazaarContextMain.ForSaleLayout} >
        <grid item-layout="length: 192" horizontal-item-alignment="middle">
          <frame *repeat={:LivestockEntries} padding="16"
            background={:~BazaarContextMain.Theme_ItemRowBackground}
            background-tint={BackgroundTint}
            pointer-enter=|~BazaarContextMain.HandleHoverLivestock(this)|
            pointer-leave=|~BazaarContextMain.HandleHoverLivestock()|
            left-click=|~BazaarContextMain.HandleSelectLivestock(this)| >
            <panel opacity={:ShopIconOpacity} layout="160px 144px" horizontal-content-alignment="middle" focusable="true">
              <image layout="content 64px" margin="0,8" sprite={:ShopIcon} tint={:ShopIconTint}/>
              <lane layout="stretch 64px" margin="0,88" orientation="horizontal">
                <image layout="48px 48px" sprite={:TradeItem} />
                <label layout="stretch 48px" text={:TradePrice} font={:TradeDisplayFont} />
              </lane>
            </panel>
          </frame>
        </grid>
      </scrollable-styled>
      <!-- info box -->
      <infobox *context={HoveredLivestock} tint={:ShopIconTint}>
        <label *if={HasRequiredBuilding} text={:LivestockName} font="dialogue"/>
        <label *if={HasRequiredBuilding} text={:Description} font="small" margin="8,0" />
      </infobox>
    </lane>
    <!-- page 2 -->
    <lane *case="2" *context={SelectedLivestock} layout="stretch content" orientation="horizontal">
      <!-- building selection -->
      <scrollable-styled layout={:~BazaarContextMain.ForSaleLayout} >
        <lane orientation="vertical">
          <lane padding="8" *repeat={:~BazaarContextMain.BazaarLocationEntries} layout="stretch content" orientation="vertical">
              <banner padding="8" text={:LocationName}/>
              <grid layout="stretch content" item-layout="length:164">
                <frame *repeat={:ValidLivestockBuildings}
                  background={:~BazaarContextMain.Theme_ItemRowBackground}
                  background-tint={BackgroundTint}
                  tooltip={:BuildingName}
                  pointer-enter=|~BazaarContextMain.HandleHoverBuilding(this)|
                  pointer-leave=|~BazaarContextMain.HandleHoverBuilding()|
                  left-click=|~BazaarContextMain.HandlePurchaseAnimal(this)| >
                  <lane layout="144px content" padding="12" orientation="vertical" focusable="true" horizontal-content-alignment="middle">
                    <image layout="120px 120px" fit="Contain" horizontal-alignment="middle" vertical-alignment="end"
                      sprite={:BuildingSprite} tint={BuildingSpriteTint}/>
                    <label font="dialogue" text={BuildingOccupant}/>
                  </lane>
                </frame>
              </grid>
          </lane>
        </lane>
      </scrollable-styled>

      <!-- infobox and confirm -->
      <infobox tint={AnimTint}>
        <lane *if={HasSkin} layout="stretch content" orientation="horizontal" margin="0,-64,0,0" horizontal-content-alignment="middle" z-index="2">
          <image layout="104px 44px" focusable="true" fit="Contain" horizontal-alignment="start" sprite={@Mods/StardewUI/Sprites/SmallLeftArrow}
            left-click=|PrevSkin()| />
          <image layout="48px 48px" sprite={@mushymato.LivestockBazaar/sprites/cursors:question} opacity={RandSkinOpacity}/>
          <image layout="104px 44px" focusable="true" fit="Contain" horizontal-alignment="end" sprite={@Mods/StardewUI/Sprites/SmallRightArrow}
            left-click=|NextSkin()| />
        </lane>
        <lane orientation="horizontal" margin="0,32">
          <textinput layout="196px 48px" text={<>BuyName}/>
          <image sprite={@mushymato.LivestockBazaar/sprites/cursors:dice} layout="32px 32px" margin="8"
            left-click=|RandomizeBuyName()| />
        </lane>
        <grid item-layout="length:80" margin="4,0" item-spacing="4,4" horizontal-item-alignment="middle">
          <frame *repeat={:AltPurchase} focusable="true"
            left-click=|~BazaarLivestockEntry.HandleSelectedPurchase(this)| >
            <image layout="content[32..] content[64..]" fit="Contain" horizontal-alignment="middle" sprite={:SpriteIcon} opacity={IconOpacity}/>
          </frame>
        </grid>
      </infobox>
    </lane>
  </frame>
</lane>

<template name="infobox">
  <lane layout="content[256..] stretch" orientation="vertical" horizontal-content-alignment="middle">
    <image layout="content content[128..]" fit="Contain" horizontal-alignment="middle" vertical-alignment="end"
      tint={&tint} sprite={AnimSprite} sprite-effects={AnimFlip} />
    <outlet/>
  </lane>
</template>

<template name="scrollable-styled">
  <scrollable peeking="128"
    layout={&layout}
    scrollbar-margin="278,0,0,-8"
    scrollbar-up-sprite={:~BazaarContextMain.Theme_ScrollUp}
    scrollbar-down-sprite={:~BazaarContextMain.Theme_ScrollDown}
    scrollbar-down-sprite={:~BazaarContextMain.Theme_ScrollDown}
    scrollbar-thumb-sprite={:~BazaarContextMain.Theme_ScrollBarFront}
    scrollbar-track-sprite={:~BazaarContextMain.Theme_ScrollBarBack}>
    <outlet/>
  </scrollable>
</template>
