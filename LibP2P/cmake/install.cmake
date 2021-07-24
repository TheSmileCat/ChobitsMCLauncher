include(GNUInstallDirs)

function(libp2p_install targets)
  install(TARGETS ${targets} EXPORT libp2pConfig
      LIBRARY DESTINATION ${CMAKE_INSTALL_LIBDIR}
      ARCHIVE DESTINATION ${CMAKE_INSTALL_LIBDIR}
      RUNTIME DESTINATION ${CMAKE_INSTALL_BINDIR}
      INCLUDES DESTINATION ${CMAKE_INSTALL_INCLUDEDIR}
      PUBLIC_HEADER DESTINATION ${CMAKE_INSTALL_INCLUDEDIR}
      FRAMEWORK DESTINATION ${CMAKE_INSTALL_PREFIX}
      )
endfunction()

install(
    DIRECTORY ${CMAKE_SOURCE_DIR}/include/libp2p
    DESTINATION ${CMAKE_INSTALL_INCLUDEDIR}
)
install(
    EXPORT libp2pConfig
    DESTINATION ${CMAKE_INSTALL_LIBDIR}/cmake/libp2p
    NAMESPACE p2p::
)
if(EXPOSE_MOCKS)
  install(
      DIRECTORY ${CMAKE_SOURCE_DIR}/test/mock/libp2p
      DESTINATION ${CMAKE_INSTALL_INCLUDEDIR}/mock
  )
endif()