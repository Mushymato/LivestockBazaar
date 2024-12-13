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
      <scrollable-styled>
        <grid layout="stretch content" item-layout="length: 192"
          horizontal-item-alignment="middle">
          <frame *repeat={:LivestockEntries} padding="16"
            background={:~BazaarContextMain.Theme_ItemRowBackground}
            background-tint={BackgroundTint}
            pointer-enter=|~BazaarContextMain.HandleHoverLivestock(this)|
            pointer-leave=|~BazaarContextMain.HandleHoverLivestock()|
            left-click=|~BazaarContextMain.HandleSelectLivestock(this)| >
            <livestock-cell/>
          </frame>
        </grid>
      </scrollable-styled>
      <!-- info box -->
      <infobox *context={HoveredLivestock} />
    </lane>
    <!-- page 2 -->
    <lane *case="2" *context={SelectedLivestock} layout="stretch content" orientation="horizontal">
      <lane layout={:~BazaarContextMain.ForSaleLayout} orientation="vertical">
        <lane orientation="horizontal">
          <image sprite={@mushymato.LivestockBazaar/sprites/cursors:dice} margin="12"
            left-click=|RandomizeBuyName()| />
          <textinput layout="196px 64px" text={<>BuyName}/>
        </lane>
        <!-- <grid *if={:HasAltPurchase}>
          <frame *repeat={:AltPurchaseEntries} padding="16"
            background={~BazaarContextMain.Theme_ItemRowBackground}
            background-tint={BackgroundTint}
            >
            <livestock-cell/>
          </frame>
        </grid> -->
        <scrollable-styled>
        <lane orientation="vertical">
          <lane padding="8" *repeat={:~BazaarContextMain.BazaarLocationEntries} layout="stretch content" orientation="vertical">
              <banner padding="8" text={:LocationName} />
              <grid layout="stretch content" item-layout="length:164">
                <frame *repeat={:ValidLivestockBuildings}
                  background={:~BazaarContextMain.Theme_ItemRowBackground}
                  background-tint={BackgroundTint}
                  tooltip={:BuildingName}
                  pointer-enter=|~BazaarContextMain.HandleHoverBuilding(this)|
                  pointer-leave=|~BazaarContextMain.HandleHoverBuilding()|
                  left-click=|~BazaarContextMain.HandlePurchaseAnimal(this)| >
                  <lane layout="144px content" padding="12" orientation="vertical" focusable="true" horizontal-content-alignment="middle">
                     <image layout="120px 120px" fit="Contain" sprite={:BuildingSprite} tint={BuildingSpriteTint} horizontal-alignment="middle" vertical-alignment="middle"/>
                    <label font="dialogue" text={BuildingOccupant}/>
                  </lane>
                </frame>
              </grid>
              <!-- <lane orientation="vertical">
                <frame *repeat={:ValidLivestockBuildings}
                  focusable="true" padding="12" layout="stretch content"
                  background={:~BazaarContextMain.Theme_ItemRowBackground}
                  background-tint={BackgroundTint}
                  pointer-enter=|~BazaarContextMain.HandleHoverBuilding(this)|
                  pointer-leave=|~BazaarContextMain.HandleHoverBuilding()|
                  left-click=|~BazaarContextMain.HandlePurchaseAnimal(this)| >
                  <label padding="8" font="dialogue" text={BuildingName}/>
                </frame>
              </lane> -->
          </lane>
        </lane>
        </scrollable-styled>
      </lane>
      <infobox/>
    </lane>
  </frame>

  <!-- spacer to counterbalance the portrait -->
  <!-- <spacer *if={:IsWidescreen} layout="192px 0px"/> -->
</lane>

<template name="livestock-cell">
  <panel opacity={:ShopIconOpacity} layout="160px 144px" horizontal-content-alignment="middle" focusable="true">
    <image layout="content 64px" margin="0,8" sprite={:ShopIcon} tint={:ShopIconTint}/>
    <lane layout="stretch 64px" margin="0,88" orientation="horizontal">
      <image layout="48px 48px" sprite={:TradeItem} />
      <label layout="stretch 48px" text={:TradePrice} font={:TradeDisplayFont} />
    </lane>
  </panel>
</template>

<template name="infobox">
  <lane layout="content[256..] stretch"  orientation="vertical" horizontal-content-alignment="middle">
    <panel layout="content content[128..]" margin="8,8,0,0" horizontal-content-alignment="middle" vertical-content-alignment="end">
      <image layout={:AnimLayout} tint={:ShopIconTint} sprite={AnimSprite} sprite-effects={AnimFlip} />
    </panel>
    <label *if={HasRequiredBuilding} text={:LivestockName} font="dialogue"/>
    <label *if={HasRequiredBuilding} text={:Description} font="small" margin="8,0" />
    <outlet/>
  </lane>
</template>

<template name="scrollable-styled">
  <scrollable layout={:~BazaarContextMain.ForSaleLayout} peeking="128"
    scrollbar-margin="278,0,0,-8"
    scrollbar-up-sprite={:~BazaarContextMain.Theme_ScrollUp}
    scrollbar-down-sprite={:~BazaarContextMain.Theme_ScrollDown}
    scrollbar-down-sprite={:~BazaarContextMain.Theme_ScrollDown}
    scrollbar-thumb-sprite={:~BazaarContextMain.Theme_ScrollBarFront}
    scrollbar-track-sprite={:~BazaarContextMain.Theme_ScrollBarBack}>
    <outlet/>
  </scrollable>
</template>
